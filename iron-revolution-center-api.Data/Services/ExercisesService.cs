using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Exercise;
using iron_revolution_center_api.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data.Service
{
    // exercise services
    public class ExercisesService : iExercisesService
    {
        #region MongoDB Configuration
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<ExercisesModel> _exercisesCollection;
        private readonly IMongoCollection<InsertExerciseDTO> _insertExercisesCollection;
        private readonly IMongoCollection<ModifyExerciseDTO> _modifyExercisesCollection;

        // method to exclude _id field
        private static ProjectionDefinition<ExercisesModel> ExcludeIdProjection()
        {
            return Builders<ExercisesModel>.Projection.Exclude("_id");
        }

        public ExercisesService(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _exercisesCollection = _mongoDatabase.GetCollection<ExercisesModel>("Exercises");
            _insertExercisesCollection = _mongoDatabase.GetCollection<InsertExerciseDTO>("Exercises");
            _modifyExercisesCollection = _mongoDatabase.GetCollection<ModifyExerciseDTO>("Exercises");
        }
        #endregion

        #region Validations
        private async Task<bool> IsExerciseIdAlreadyUsed(string exerciseId)
        {
            try
            {
                // check exercise
                var exerciseExists = await _exercisesCollection
                    .CountDocumentsAsync(exercise => exercise.Ejercicio_Id == exerciseId);

                // validate existence
                return exerciseExists > 0;
            } catch {
                // if not in used
                return false;
            }
        }
        private async Task<bool> IsNameAlreadyUsed(string exerciseName)
        {
            try
            {
                // check ecercise name
                var count = await _exercisesCollection
                    .CountDocumentsAsync(exercise => exercise.Nombre == exerciseName);

                // validate existence
                return count > 0;
            } catch {
                // if not in used
                return false;
            }
        }
        #endregion

        #region ListExercises
        public async Task<IEnumerable<ExercisesModel>> ListExercises(string exerciseType)
        {
            try
            {
                var filterBuilder = Builders<ExercisesModel>.Filter;
                var filter = new List<FilterDefinition<ExercisesModel>>();

                if (!string.IsNullOrEmpty(exerciseType))
                    filter.Add(filterBuilder.Eq(exercise => exercise.Tipo, exerciseType));

                var filters = filter.Any() ? filterBuilder.And(filter) : filterBuilder.Empty;

                // get exercises
                return await _exercisesCollection
                    .Find(filters)
                    .Project<ExercisesModel>(ExcludeIdProjection())
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al mostrar ejercicios. {ex}");
            }
        }
        #endregion

        #region RegisterExercises
        public async Task<InsertExerciseDTO> InsertExercises(newExerciseDTO exerciseDTO)
        {
            if (string.IsNullOrEmpty(exerciseDTO.Nombre))
                throw new ArgumentException($"El nombre no puede estar vacío.");
            if (await IsNameAlreadyUsed(exerciseDTO.Nombre))
                throw new ArgumentException($"Nombre: {exerciseDTO.Nombre} ya en uso");
            if (exerciseDTO.Series <= 0) 
                throw new ArgumentException($"Series no puede estar vacía o ser igual o menor a 0. {nameof(exerciseDTO.Series)}");
            if (exerciseDTO.Repeticiones <= 0) 
                throw new ArgumentException($"Repeticiones no puede estar vacía o ser igual o menor a 0. {nameof(exerciseDTO.Repeticiones)}");
            try
            {
                // generate a unique id
                string exerciseId;
                do
                {
                    string num = new Random().Next(1, 1000).ToString("D3");
                    exerciseId = $"E{num}";
                } while (await IsExerciseIdAlreadyUsed(exerciseId));

                // insert exercise
                var newExercise = new InsertExerciseDTO
                {
                    Ejercicio_Id = exerciseId,
                    Foto = exerciseDTO.Foto,
                    Nombre = exerciseDTO.Nombre,
                    Tipo = exerciseDTO.Tipo,
                    Descripcion = exerciseDTO.Descripcion,
                    Series = exerciseDTO.Series,
                    Repeticiones = exerciseDTO.Repeticiones
                };

                // check if is not null
                if (newExercise == null)
                    throw new ArgumentException("Registro fallido.");

                // insert
                await _insertExercisesCollection.InsertOneAsync(newExercise);

                //exercise
                return newExercise;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar ejercicio. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region ModifyExercises
        public async Task<ExercisesModel> ModifyExercises(string exerciseId, ModifyExerciseDTO exerciseDTO)
        {
            if (!await IsExerciseIdAlreadyUsed(exerciseId))
                throw new ArgumentException($"El ejercicio con ID: {exerciseId} no existe.");
            try
            {
                // create update definitions
                var updateBuilder = Builders<ExercisesModel>.Update;
                var updateDefinitions = new List<UpdateDefinition<ExercisesModel>>();

                // modify not null field
                if (exerciseDTO.Foto != null) // photo
                    updateDefinitions.Add(updateBuilder
                                     .Set(exercise => exercise.Foto, exerciseDTO.Foto));
                if (!string.IsNullOrEmpty(exerciseDTO.Nombre)) // name
                {
                    if (await IsNameAlreadyUsed(exerciseDTO.Nombre))
                        throw new ArgumentException($"El nombre del ejercicio: {exerciseDTO.Nombre} ya está en uso.");

                    updateDefinitions.Add(updateBuilder
                                     .Set(exercise => exercise.Nombre, exerciseDTO.Nombre));
                }
                if (!string.IsNullOrEmpty(exerciseDTO.Tipo)) // type
                    updateDefinitions.Add(updateBuilder
                                     .Set(exercise => exercise.Tipo, exerciseDTO.Tipo));
                if (!string.IsNullOrEmpty(exerciseDTO.Descripcion)) // description
                    updateDefinitions.Add(updateBuilder
                                     .Set(exercise => exercise.Descripcion, exerciseDTO.Descripcion));
                if (exerciseDTO.Series.HasValue && exerciseDTO.Series > 0) // series
                    updateDefinitions.Add(updateBuilder
                                     .Set(exercise => exercise.Series, exerciseDTO.Series));
                if (exerciseDTO.Repeticiones.HasValue && exerciseDTO.Series > 0) // repitions
                    updateDefinitions.Add(updateBuilder
                                     .Set(exercise => exercise.Repeticiones, exerciseDTO.Repeticiones));

                // verification
                if (!updateDefinitions.Any())
                    throw new Exception("No se proporcionaron campos válidos para modificar.");

                // combine to a single
                var combine = updateBuilder.Combine(updateDefinitions);

                // modify
                var filter = Builders<ExercisesModel>
                    .Filter
                    .Eq(exercise => exercise.Ejercicio_Id, exerciseId);
                // result of the modify
                var update = await _exercisesCollection
                    .UpdateOneAsync(filter, combine);
                // check if the update was successful
                if (update.ModifiedCount == 0)
                    throw new ArgumentException("Error al modificar ejercicio.");

                // exercise
                return await _exercisesCollection
                    .Find(exercise => exercise.Ejercicio_Id == exerciseId)
                    .Project<ExercisesModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar trabajador. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region DeleteExercises
        public async Task<ExercisesModel> DeleteExercises(string exerciseId)
        {
            if (!await IsExerciseIdAlreadyUsed(exerciseId))
                throw new ArgumentException($"El ejercicio con ID: {exerciseId} no existe.");
            try
            {
                // exercise
                var exercise = await _exercisesCollection
                    .Find(exercise => exercise.Ejercicio_Id == exerciseId)
                    .Project<ExercisesModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // check if is not null
                if (exercise == null)
                    throw new ArgumentException("Ejercicio no encontrado.");

                // delete
                await _exercisesCollection.DeleteOneAsync(exercise => exercise.Ejercicio_Id == exerciseId);

                // exercise
                return exercise;
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
