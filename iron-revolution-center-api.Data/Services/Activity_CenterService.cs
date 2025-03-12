using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Activity_Center;
using iron_revolution_center_api.DTOs.Staff;
using iron_revolution_center_api.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace iron_revolution_center_api.Data.Service
{
    public class Activity_CenterService : iActivity_CenterService
    {
        //#region MongoDB Configuration
        //private readonly IMongoDatabase _mongoDatabase;
        //private readonly IMongoCollection<Activity_CenterModel> _activityCenterCollection;
        //private readonly IMongoCollection<EntryClientDTO> _entryClientCollection;
        //private readonly IMongoCollection<ExitClientDTO> _exitClientCollection;
        //private readonly IMongoCollection<ClientsModel> _clientsCollection;
        //private readonly IMongoCollection<Branches_OfficeModel> _branchesOfficeCollection;

        //// method to exclude _id field
        //private static ProjectionDefinition<Activity_CenterModel> ExcludeIdProjection()
        //{
        //    return Builders<Activity_CenterModel>.Projection.Exclude("_id");
        //}

        //public Activity_CenterService(IMongoDatabase mongoDatabase)
        //{
        //    _mongoDatabase = mongoDatabase;
        //    _activityCenterCollection = _mongoDatabase.GetCollection<Activity_CenterModel>("Activity_Center");
        //    _entryClientCollection = _mongoDatabase.GetCollection<EntryClientDTO>("Activity_Center");
        //    _exitClientCollection = _mongoDatabase.GetCollection<ExitClientDTO>("Activity_Center");
        //    _clientsCollection = _mongoDatabase.GetCollection<ClientsModel>("Clients");
        //    _branchesOfficeCollection = _mongoDatabase.GetCollection<Branches_OfficeModel>("Branches_Office");
        //}
        //#endregion

        //#region Validations
        //private async Task<bool> HasActiveMembership(string NIP)
        //{
        //    try
        //    {
        //        // get client
        //        var client = await _clientsCollection
        //            .Find(client => client.NIP == NIP)
        //            .FirstOrDefaultAsync();

        //        if (client == null)
        //            throw new ArgumentException("Cliente no encontrado.");

        //        if (client.Estado == false)
        //            throw new ArgumentException("Estado finalizado.");

        //        // check if membership is still valid
        //        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        //        if (client.Fecha_Fin.GetValueOrDefault() < currentDate)
        //            throw new ArgumentException("Membresía finalizada.");

        //        return true;
        //    } catch {
        //        return false;
        //    }
        //}

        //private async Task<bool> IsClientRegistered(string NIP)
        //{
        //    try
        //    {
        //        var clientCount = await _clientsCollection
        //            .CountDocumentsAsync(client => client.NIP == NIP);
        //        return clientCount > 0;
        //    } catch {
        //        return false;
        //    }
        //}

        //private async Task<bool> IsBranchOfficeValid(string branchID)
        //{
        //    try
        //    {
        //        var branchCount = await _branchesOfficeCollection
        //            .CountDocumentsAsync(branchOffice => branchOffice.Sucursal_ID == branchID);
        //        return branchCount > 0;
        //    } catch {
        //        return false;
        //    }
        //}
        //#endregion

        //#region ListActivity
        //public async Task<IEnumerable<Activity_CenterModel>> ListActivity()
        //{
        //    try
        //    {
        //        // get clients
        //        var clients = await _clientsCollection
        //            .Find(_ => true)
        //            .ToListAsync();

        //        // get branches office
        //        var branches = await _branchesOfficeCollection
        //            .Find(_ => true)
        //            .ToListAsync();

        //        // get activities
        //        var activities = await _activityCenterCollection
        //            .Find(_ => true)
        //            .ToListAsync();

        //        var activityCenter = activities.Select(activity => new Activity_CenterModel
        //        {
        //            Cliente = clients.FirstOrDefault(),
        //            Entrada = activity.Entrada,
        //            Salida = activity.Salida,
        //            Sucursal = branches.FirstOrDefault()
        //        }).ToList();

        //        return activityCenter;
        //    } catch (MongoException ex) {
        //        throw new InvalidOperationException($"Error al mostrar actividad. {ex}");
        //    }
        //}
        //#endregion

        //#region RegisterEntry
        //public async Task<bool> RegisterEntry(string NIP, string branchOffice)
        //{
        //    if (string.IsNullOrWhiteSpace(NIP))
        //        throw new ArgumentException("El NIP no puede estar vacío.");
        //    if (!await IsClientRegistered(NIP))
        //        throw new ArgumentException($"NIP {NIP} no existe.");
        //    if (string.IsNullOrWhiteSpace(branchOffice))
        //        throw new ArgumentException("Sucursal no puede estar vacío.");
        //    if (!await IsBranchOfficeValid(branchOffice))
        //        throw new ArgumentException($"Sucursal con el ID {branchOffice} no existe.");
        //    try
        //    {

        //        // get client
        //        var client = await _clientsCollection
        //            .Find(client => client.NIP == NIP)
        //            .FirstOrDefaultAsync();

        //        if (client == null)
        //            throw new ArgumentException("Cliente no encontrado.");

        //        if (client.Estado == false)
        //            throw new ArgumentException("Estado finalizado.");

        //        // check if membership is still valid
        //        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        //        Console.WriteLine($"currentDate: {currentDate}. fecha fin: {client.Fecha_Fin}");
        //        if (client.Fecha_Fin < currentDate)
        //            throw new ArgumentException("Membresía finalizada.");

        //        // check if the client has an active membership
        //        if (!await HasActiveMembership(NIP))
        //            throw new ArgumentException("No apto para entrar.");

        //        var clientEnty = new EntryClientDTO
        //        {
        //            Cliente = NIP,
        //            Entrada = DateTime.UtcNow,
        //            Salida = DateTime.UtcNow.AddHours(24),
        //            Sucursal = branchOffice
        //        };

        //        await _entryClientCollection.InsertOneAsync(clientEnty);

        //        return true;
        //    } catch (MongoException ex) {
        //        throw new InvalidOperationException($"Error al registrar entrada. {ex}");
        //    } catch (ArgumentException ex) {
        //        throw new ArgumentException($"Error: {ex}");
        //    }
        //}
        //#endregion

        //#region RegisterExit
        //public async Task<bool> RegisterExit(string NIP)
        //{
        //    if (string.IsNullOrWhiteSpace(NIP))
        //        throw new ArgumentException("NIP no puede estar vacío.");
        //    if (!await IsClientRegistered(NIP))
        //        throw new ArgumentException($"NIP {NIP} no existe.");
        //    try
        //    {
        //        // find client entry
        //        var clientEntry = await _entryClientCollection
        //            .Find(client => client.Cliente == NIP && DateOnly.FromDateTime(client.Entrada) == DateOnly.FromDateTime(DateTime.Now))
        //            .FirstOrDefaultAsync();

        //        if (clientEntry == null)
        //            throw new ArgumentException("Entrada inexistente.");

        //        var filter = Builders<Activity_CenterModel>
        //                        .Filter.Eq(client => client.Cliente.NIP, NIP)
        //                     & Builders<Activity_CenterModel>
        //                        .Filter.Eq(client => client.Entrada, clientEntry.Entrada);

        //        // insert exit time
        //        var update = Builders<Activity_CenterModel>.Update
        //            .Set(client => client.Salida, DateTime.UtcNow);

        //        await _activityCenterCollection.UpdateOneAsync(filter, update);

        //        return true;
        //    } catch (MongoException ex) {
        //        throw new InvalidOperationException($"Error al registrar salida. {ex}");
        //    } catch (ArgumentException ex) {
        //        throw new ArgumentException($"Error: {ex}");
        //    }
        //}
        //#endregion
    }
}
