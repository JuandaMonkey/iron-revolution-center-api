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
    public class Branches_OfficeService : iBranches_OfficeService
    {
        #region MongoDB Configuration
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<Branches_OfficeModel> _branchesOfficeCollection;
        private IMongoCollection<InsertBranche_OfficeDTO> _insertBranchesOfficeCollection;
        private IMongoCollection<InsertBranche_OfficeDTO> _modifyBranchesOfficeCollection;

        public Branches_OfficeService(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _branchesOfficeCollection = _mongoDatabase.GetCollection<Branches_OfficeModel>("Branches_Office");
            _insertBranchesOfficeCollection = _mongoDatabase.GetCollection<InsertBranche_OfficeDTO>("Branches_Office");
            _modifyBranchesOfficeCollection = _mongoDatabase.GetCollection<InsertBranche_OfficeDTO>("Branches_Office");
        }
        #endregion

        #region Validations
        private async Task<bool> ValidateBranchOfficeID(string branchID)
        {
            try
            {
                // check branch office exists
                var branchOfficeExists = await _branchesOfficeCollection
                    .CountDocumentsAsync(branchOffice => branchOffice.Branche_ID == branchID);

                // validate existence
                return branchOfficeExists > 0;
            } catch {
                // if not in used
                return false;
            }
        }
        #endregion

        #region ListBranch_Office
        public async Task<IEnumerable<Branches_OfficeModel>> ListBranch_Office()
        {
            try
            {
                // get the collection branches office
                return await _branchesOfficeCollection
                    .Find(FilterDefinition<Branches_OfficeModel>.Empty)
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al mostrar sucursales. {ex}");
            }
        }
        #endregion

        #region RegisterBranch_Office
        public async Task<InsertBranche_OfficeDTO> RegisterBranch_Office(InsertBranche_OfficeDTO branchOfficeDTO)
        {
            if (string.IsNullOrEmpty(branchOfficeDTO.Name)) // field verification
                throw new ArgumentException($"El nombre no puede estar vacío. {nameof(branchOfficeDTO.Name)}");
            if (string.IsNullOrEmpty(branchOfficeDTO.Location)) // field verification
                throw new ArgumentException($"La ubicación no puede estar vacía. {nameof(branchOfficeDTO.Location)}");
            try
            {
                // generate a unique id
                string branchOfficeID;
                string num;
                do
                {
                    num = new Random().Next(1, 1000).ToString("D3");
                    branchOfficeID = $"B{num}";
                }
                // check if id is already used
                while (await ValidateBranchOfficeID(branchOfficeID) == true);

                // register branch office
                var newBranchOffice = new InsertBranche_OfficeDTO
                {
                    Branche_ID = branchOfficeID,
                    Name = branchOfficeDTO.Name,
                    Location = branchOfficeDTO.Location
                };

                if (newBranchOffice == null)
                    throw new ArgumentException("Registro fallido.");

                // insert
                await _insertBranchesOfficeCollection.InsertOneAsync(newBranchOffice);

                // branch office
                return newBranchOffice;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar sucursal. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region ModifyBranch_Office
        public async Task<Branches_OfficeModel> ModifyBranch_Office(string branchOfficeID, ModifyBranche_OfficeDTO branchOfficeDTO)
        {
            if (string.IsNullOrEmpty(branchOfficeID))
                throw new ArgumentException($"El ID de la sucursal no puede estar vacío. {nameof(branchOfficeID)}");
            if (!await ValidateBranchOfficeID(branchOfficeID))
                throw new ArgumentException($"El ID: {nameof(branchOfficeID)} no existe.");
            try
            {
                // create update definitions
                var updateBuilder = Builders<Branches_OfficeModel>.Update;
                var updateDefinitions = new List<UpdateDefinition<Branches_OfficeModel>>();

                // modify not null field
                if (!string.IsNullOrEmpty(branchOfficeDTO.Name)) // name
                    updateDefinitions.Add(updateBuilder.Set(branch => branch.Name, branchOfficeDTO.Name));
                if (!string.IsNullOrEmpty(branchOfficeDTO.Location)) // location 
                    updateDefinitions.Add(updateBuilder.Set(branch => branch.Location, branchOfficeDTO.Location));

                // verification
                if (!updateDefinitions.Any())
                    throw new Exception("No se proporcionaron campos válidos para modificar.");

                // combine to single
                var combine = updateBuilder.Combine(updateDefinitions);

                // modify
                var filter = Builders<Branches_OfficeModel>
                    .Filter
                    .Eq(branch => branch.Branche_ID, branchOfficeID);
                // resutl of modify
                var update = await _branchesOfficeCollection
                    .UpdateOneAsync(filter, combine);
                // check if the update was successful
                if (update.ModifiedCount == 0)
                    throw new ArgumentException("Error al modificar sucursal.");

                // branch office
                Branches_OfficeModel branchOffice = await _branchesOfficeCollection
                    .Find(branch => branch.Branche_ID == branchOfficeID)
                    .FirstOrDefaultAsync();

                // branch office
                return branchOffice;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar sucursal. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region DeleteBranch_Office
        public async Task<Branches_OfficeModel> DeleteBranch_Office(string branchOfficeID)
        {
            if (string.IsNullOrEmpty(branchOfficeID))
                throw new ArgumentException($"El ID de la sucursal no puede estar vacío. {nameof(branchOfficeID)}");
            if (! await ValidateBranchOfficeID(branchOfficeID))
                throw new ArgumentException($"El ID: {branchOfficeID} no existe.");
            try
            {
                // branch office
                var branchOffice = await _branchesOfficeCollection
                    .Find(branch => branch.Branche_ID == branchOfficeID)
                    .FirstOrDefaultAsync();

                // check if is not null
                if (branchOffice == null)
                    throw new ArgumentException("Sucursal no encontrada.");

                // delete
                await _branchesOfficeCollection.DeleteOneAsync(branch => branch.Branche_ID == branchOfficeID);

                // branch office
                return branchOffice;
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
