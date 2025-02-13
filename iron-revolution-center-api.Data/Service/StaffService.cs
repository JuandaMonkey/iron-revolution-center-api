using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Staff;
using iron_revolution_center_api.DTOs.User;
using iron_revolution_center_api.Models;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data.Service
{
    public class StaffService : iStaffService
    {
        #region MongoDB Configuration
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<StaffModel> _staffCollection;
        private readonly IMongoCollection<Branches_OfficeModel> _branchesOfficeCollection;
        private readonly IMongoCollection<RegisterStaffDTO> _registerStaffCollection;
        private readonly IMongoCollection<ModifyStaffDTO> _modifyStaffCollection;

        // method to exclude _id field
        private static ProjectionDefinition<StaffModel> ExcludeIdProjection()
        {
            return Builders<StaffModel>.Projection.Exclude("_id");
        }

        public StaffService(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _staffCollection = _mongoDatabase.GetCollection<StaffModel>("Staff");
            _branchesOfficeCollection = _mongoDatabase.GetCollection<Branches_OfficeModel>("Branches_Office");
            _registerStaffCollection = _mongoDatabase.GetCollection<RegisterStaffDTO>("Staff");
            _modifyStaffCollection = _mongoDatabase.GetCollection<ModifyStaffDTO>("Staff");
        }
        #endregion

        #region Validations
        private async Task<bool> ValidateStaffNIP(string NIP)
        {
            try
            {
                // check staff nip
                var NIPIsAlreadyUsed = await _staffCollection
                    .CountDocumentsAsync(staff => staff.NIP == NIP);

                // validate existence
                return NIPIsAlreadyUsed > 0;
            } catch {
                // if not in used
                return false;
            }
        }

        private async Task<bool> ValidateBranchOfficeExists(string brachID)
        {
            try
            {
                // check branch
                var BranchOfficeExists = await _branchesOfficeCollection
                    .CountDocumentsAsync(branchOffice => branchOffice.Branche_ID == brachID);

                // validate existence
                return BranchOfficeExists > 0;
            } catch {
                return false;
            }
        }

        private async Task<string> GetBranchOfficeNameByID(string branchID)
        {
            try
            {
                // check branch office name
                var branchOffice = await _branchesOfficeCollection
                    .Find(branchOffice => branchOffice.Branche_ID == branchID)
                    .FirstOrDefaultAsync();

                // validate existence
                return branchOffice?.Name;
            } catch {
                return null;
            }
        }

        private async Task<bool> ValidateStaffPhone(string phone)
        {
            try
            {
                // check client number
                var phoneIsAlreadyUsed = await _staffCollection
                    .CountDocumentsAsync(staff => staff.Phone == phone);

                // validate existence
                return phoneIsAlreadyUsed > 0;
            }
            catch
            {
                // if not in used
                return false;
            }
        }
        #endregion

        #region ListStaff
        public async Task<IEnumerable<StaffModel>> ListStaff()
        {
            try
            {
                // get the collection staff
                return await _staffCollection
                    .Find(FilterDefinition<StaffModel>.Empty)
                    .Project<StaffModel>(ExcludeIdProjection())
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al mostrar trabajadores. {ex}");
            }
        }
        #endregion

        #region ListStaffByBranchOffice
        public async Task<IEnumerable<StaffModel>> ListStaffByBranchOffice(string branchID)
        {
            if (string.IsNullOrEmpty(branchID)) // field verification
                throw new ArgumentException("La sucursal no puede estar vacío.", nameof(branchID));
            if (!await ValidateBranchOfficeExists(branchID)) // field verification
            {
                var branchOfficeName = await GetBranchOfficeNameByID(branchID);
                throw new ArgumentException($"La sucursal: {branchOfficeName} no existe.");
            }
            try
            {
                // get users
                return await _staffCollection
                    .Find(staff => staff.Branch_Office == branchID)
                    .Project<StaffModel>(ExcludeIdProjection())
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al al mostrar trabajadores. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region GetStaffByNIP
        public async Task<IEnumerable<StaffModel>> GetStaffByNIP(string NIP)
        {
            if (string.IsNullOrEmpty(NIP)) // field verification
                throw new ArgumentException("El NIP del trabajador no puede estar vacio.", nameof(NIP));
            if (!await ValidateStaffNIP(NIP)) // field verificartion
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            try
            {
                // get the collection
                return await _staffCollection
                    .Find(staff => staff.NIP == NIP)
                    .Project<StaffModel>(ExcludeIdProjection())
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al al mostrar tranajador. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region RegisterStaff
        public async Task<RegisterStaffDTO> RegisterStaff(RegisterStaffDTO staffDTO)
        {
            if (string.IsNullOrEmpty(staffDTO.Full_Name)) // field verification
                throw new ArgumentException("El nombre no puede estar vacio.", nameof(staffDTO.Full_Name));
            if (string.IsNullOrEmpty(staffDTO.Phone)) // field verification
                throw new ArgumentException("El número de celular no puede estar vacio.", nameof(staffDTO.Phone));
            if (await ValidateStaffPhone(staffDTO.Phone) == true) // field verification
                throw new ArgumentException($"Número de celular: {staffDTO.Phone} ya en uso.");
            if (string.IsNullOrEmpty(staffDTO.Branch_Office)) // field verification
                throw new ArgumentException("La sucursal no puede estar vacio.", nameof(staffDTO.Branch_Office));
            if (!await ValidateBranchOfficeExists(staffDTO.Branch_Office)) // field verification
            {
                var branchOfficeName = await GetBranchOfficeNameByID(staffDTO.Branch_Office);
                throw new ArgumentException($"La sucursal: {branchOfficeName} no existe:");
            }
            try
            {
                // generate a unique nip
                string NIP;
                string num;
                do
                {
                    num = new Random().Next(1, 1000).ToString("D3");
                    NIP = $"S{num}";
                }
                // check if nip is already used
                while (await ValidateStaffNIP(NIP) == true);

                // register staff
                var newStaff = new RegisterStaffDTO
                {
                    NIP = NIP,
                    Photo = staffDTO.Photo,
                    Full_Name = staffDTO.Full_Name,
                    Phone = staffDTO.Phone,
                    Branch_Office = staffDTO.Branch_Office
                };

                // check if is not null
                if (newStaff == null)
                    throw new ArgumentException("Registro fallido.");

                // register
                await _registerStaffCollection.InsertOneAsync(newStaff);

                // staff
                return newStaff;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar trabajador. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region ModifyStaff
        public async Task<StaffModel> ModifyStaff(string NIP, ModifyStaffDTO staffDTO)
        {
            if (string.IsNullOrEmpty(NIP)) // field verification
                throw new ArgumentException("El NIP del trabajador no puede estar vacio.", nameof(NIP));
            if (!await ValidateStaffNIP(NIP)) // field verification
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            try
            {
                // create update definitions 
                var updateBuilder = Builders<StaffModel>.Update;
                var updateDefinitions = new List<UpdateDefinition<StaffModel>>();

                // modify not null field
                if (!string.IsNullOrEmpty(staffDTO.Photo)) // photo
                    updateDefinitions.Add(updateBuilder.Set(staff => staff.Photo, staffDTO.Photo));
                if (!string.IsNullOrEmpty(staffDTO.Full_Name)) // full name
                    updateDefinitions.Add(updateBuilder.Set(staff => staff.Full_Name, staffDTO.Full_Name));
                if (!string.IsNullOrEmpty(staffDTO.Phone)) // phone
                {
                    if (await ValidateStaffPhone(staffDTO.Phone) == true) // field verification
                        throw new ArgumentException($"Número de celular: {staffDTO.Phone} ya en uso.");
                    updateDefinitions.Add(updateBuilder.Set(staff => staff.Phone, staffDTO.Phone));
                }
                if (!string.IsNullOrEmpty(staffDTO.Branch_Office)) // branch office
                {
                    // check branch office exists
                    if (!await ValidateBranchOfficeExists(staffDTO.Branch_Office))
                    {
                        var branchOfficeName = await GetBranchOfficeNameByID(staffDTO.Branch_Office);
                        throw new ArgumentException($"La sucursal: {branchOfficeName} no existe:");
                    }
                    updateDefinitions.Add(updateBuilder.Set(staff => staffDTO.Branch_Office, staffDTO.Branch_Office));
                }

                // verification
                if (!updateDefinitions.Any())
                    throw new Exception("No se proporcionaron campos válidos para modificar.");

                // combine to a single
                var combine = updateBuilder.Combine(updateDefinitions);

                // modify
                var filter = Builders<StaffModel>
                    .Filter
                    .Eq(staff => staff.NIP, NIP);
                // result of the modify
                var update = await _staffCollection
                    .UpdateOneAsync(filter, combine);
                // check if the update was successful
                if (update.ModifiedCount == 0)
                    throw new ArgumentException("Error al modificar trabajador");

                // staff
                StaffModel staff = await _staffCollection
                    .Find(staff => staff.NIP == NIP)
                    .Project<StaffModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // staff
                return staff;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar trabajador. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region DeleteStaff
        public async Task<StaffModel> DeleteStaff(string NIP)
        {
            if (string.IsNullOrEmpty(NIP)) // field verification
                throw new ArgumentException("El NIP del trabajador no puede estar vacio.", nameof(NIP));
            if (!await ValidateStaffNIP(NIP))
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            try
            {
                // staff
                var staff = await _staffCollection
                    .Find(staff => staff.NIP == NIP)
                    .FirstOrDefaultAsync();

                // check if is not null
                if (staff == null)
                    throw new ArgumentException("Trabajador no encontrado.");

                // delete
                await _staffCollection.DeleteOneAsync(staff => staff.NIP == NIP);

                // user
                return staff;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar trabajador. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion
    }
}
