using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Branch_Office;
using iron_revolution_center_api.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data.Service
{
    public class BranchesService : iBranchesService
    {
        #region MongoDB Configuration
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<BranchesModel> _branchesCollection;
        private IMongoCollection<InsertBranchDTO> _insertBranchesCollection;
        private IMongoCollection<ModifyBranchDTO> _modifyBranchesCollection;

        // method to exclude _id field
        private static ProjectionDefinition<BranchesModel> ExcludeIdProjection()
        {
            return Builders<BranchesModel>.Projection.Exclude("_id");
        }

        public BranchesService(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _branchesCollection = _mongoDatabase.GetCollection<BranchesModel>("Branches");
            _insertBranchesCollection = _mongoDatabase.GetCollection<InsertBranchDTO>("Branches");
            _modifyBranchesCollection = _mongoDatabase.GetCollection<ModifyBranchDTO>("Branches");
        }
        #endregion

        #region Validations
        private async Task<bool> IsBranchIdAlreadyUsed(string branchId)
        {
            try
            {
                // check branch
                var branchExists = await _branchesCollection
                    .CountDocumentsAsync(branch => branch.Sucursal_Id == branchId);

                // validate existence
                return branchExists > 0;
            } catch {
                // if not in used
                return false;
            }
        }

        private async Task<bool> IsBranchLocationAlreadyUsed(string branchLocation)
        {
            try
            {
                // check branch
                var branchOfficeExists = await _branchesCollection
                    .CountDocumentsAsync(branchOffice => branchOffice.Ubicacion == branchLocation);

                // validate existence
                return branchOfficeExists > 0;
            } catch {
                // if not in used
                return false;
            }
        }
        #endregion

        #region ListBranches
        public async Task<IEnumerable<BranchesModel>> ListBranches()
        {
            try
            {
                // get branches 
                return await _branchesCollection
                    .Find(FilterDefinition<BranchesModel>.Empty)
                    .Project<BranchesModel>(ExcludeIdProjection())
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al mostrar sucursales. {ex}");
            }
        }
        #endregion

        #region RegisterBranch_Office
        public async Task<InsertBranchDTO> RegisterBranch(InsertBranchDTO branchDTO)
        {
            if (string.IsNullOrEmpty(branchDTO.Ubicacion))
                throw new ArgumentException($"La ubicación de la sucursal no puede estar vacío.");
            if (await IsBranchLocationAlreadyUsed(branchDTO.Ubicacion))
                throw new ArgumentException($"La ubicación: {branchDTO.Ubicacion} ya esta en uso.");
            try
            {
                // generate a unique id
                string branchId;
                do
                {
                    string num = new Random().Next(1, 1000).ToString("D3");
                    branchId = $"S{num}";
                } while (await IsBranchIdAlreadyUsed(branchId));

                branchDTO.Sucursal_Id = branchId;

                // insert branch
                var newBranche = new InsertBranchDTO
                {
                    Sucursal_Id = branchDTO.Sucursal_Id,
                    Nombre = branchDTO.Nombre,
                    Ubicacion = branchDTO.Ubicacion
                };

                // check if is not null
                if (newBranche == null)
                    throw new ArgumentException("Registro fallido.");

                // insert
                await _insertBranchesCollection.InsertOneAsync(newBranche);

                // branch office
                return newBranche;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar sucursal. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region ModifyBranch
        public async Task<BranchesModel> ModifyBranch(string branchId, ModifyBranchDTO branchDTO)
        {
            if (string.IsNullOrEmpty(branchId))
                throw new ArgumentException($"El ID de la sucursal no puede estar vacío.");
            if (!await IsBranchIdAlreadyUsed(branchId))
                throw new ArgumentException($"El ID: {branchId} no existe.");
            if (string.IsNullOrEmpty(branchDTO.Ubicacion))
                throw new ArgumentException($"La ubicación de la sucursal no puede estar vacío.");
            if (await IsBranchLocationAlreadyUsed(branchDTO.Ubicacion))
                throw new ArgumentException($"La ubicación: {branchDTO.Ubicacion} ya esta en uso.");
            try
            {
                // create update definitions
                var updateBuilder = Builders<BranchesModel>.Update;
                var updateDefinitions = new List<UpdateDefinition<BranchesModel>>();

                // modify not null field
                if (!string.IsNullOrEmpty(branchDTO.Nombre)) // name
                    updateDefinitions.Add(updateBuilder
                                     .Set(branch => branch.Nombre, branchDTO.Nombre));
                if (!string.IsNullOrEmpty(branchDTO.Ubicacion)) // location 
                    updateDefinitions.Add(updateBuilder
                                     .Set(branch => branch.Ubicacion, branchDTO.Ubicacion));

                // verification
                if (!updateDefinitions.Any())
                    throw new Exception("No se proporcionaron campos válidos para modificar.");

                // combine to single
                var combine = updateBuilder.Combine(updateDefinitions);

                // modify
                var filter = Builders<BranchesModel>
                    .Filter
                    .Eq(branch => branch.Sucursal_Id, branchId);
                // resutl of the modification
                var update = await _branchesCollection
                    .UpdateOneAsync(filter, combine);
                // check if the update was successful
                if (update.ModifiedCount == 0)
                    throw new ArgumentException("Error al modificar sucursal.");

                // branch
                return await _branchesCollection
                    .Find(branch => branch.Sucursal_Id == branchId)
                    .Project<BranchesModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar sucursal. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region DeleteBranch
        public async Task<BranchesModel> DeleteBranch(string branchId)
        {
            if (string.IsNullOrEmpty(branchId))
                throw new ArgumentException($"El ID de la sucursal no puede estar vacío.");
            if (!await IsBranchIdAlreadyUsed(branchId))
                throw new ArgumentException($"El ID: {branchId} no existe.");
            try
            {
                // branch
                var branch = await _branchesCollection
                    .Find(branch => branch.Sucursal_Id == branchId)
                    .Project<BranchesModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // check if is not null
                if (branch == null)
                    throw new ArgumentException("Sucursal no encontrada.");

                // delete
                await _branchesCollection.DeleteOneAsync(branch => branch.Sucursal_Id == branchId);

                // branch
                return branch;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al eliminar sucursal. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion
    }
}
