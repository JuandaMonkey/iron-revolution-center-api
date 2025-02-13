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
        private async Task<bool> ValidateExerciseID(string exerciseID)
        {
            try
            {
                // check exercise id
                var exerciseExists = await _exercisesCollection
                    .CountDocumentsAsync(exercise => exercise.Exercise_ID == exerciseID);

                // validate existence
                return exerciseExists > 0;
            } catch {
                // if not in used
                return false;
            }
        }
        private async Task<bool> ValidateExerciseName(string name)
        {
            try
            {
                // check ecercise name
                var count = await _exercisesCollection
                    .CountDocumentsAsync(exercise => exercise.Name == name);

                // validate existence
                return count > 0;
            } catch {
                // if not in used
                return false;
            }
        }
        #endregion

        #region ListExercises
        public async Task<IEnumerable<ExercisesModel>> ListExercises()
        {
            try
            {
                // get the collection exercises
                return await _exercisesCollection
                    .Find(FilterDefinition<ExercisesModel>.Empty)
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al mostrar ejercicios. {ex}");
            }
        }
        #endregion

        #region RegisterExercises
        public async Task<InsertExerciseDTO> RegisterExercises(InsertExerciseDTO exerciseDTO)
        {
            if (string.IsNullOrEmpty(exerciseDTO.Name)) // field verification
                throw new ArgumentException($"El nombre no puede estar vacío. {nameof(exerciseDTO.Name)}");
            if (string.IsNullOrEmpty(exerciseDTO.Description)) // field verification
                throw new ArgumentException($"La descripción no puede estar vacía. {nameof(exerciseDTO.Description)}");
            if (exerciseDTO.Series <= 0) // field verification
                throw new ArgumentException($"Series no puede estar vacía o ser igual o menor a 0. {nameof(exerciseDTO.Series)}");
            if (exerciseDTO.Repetitions <= 0) // field verification
                throw new ArgumentException($"Repeticiones no puede estar vacía o ser igual o menor a 0. {nameof(exerciseDTO.Series)}");
            try
            {
                // generate a unique id
                string exerciseID;
                string num;
                do
                {
                    num = new Random().Next(1, 1000).ToString("D3");
                    exerciseID = $"E{num}";
                }
                // check if id is already used
                while (await ValidateExerciseID(exerciseID) == true);

                // register exercise
                var newExercise = new InsertExerciseDTO
                {
                    Exercise_ID = exerciseID,
                    Photo = exerciseDTO.Photo,
                    Name = exerciseDTO.Name,
                    Description = exerciseDTO.Description,
                    Series = exerciseDTO.Series,
                    Repetitions = exerciseDTO.Repetitions
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
        public async Task<ExercisesModel> ModifyExercises(string exerciseID, ModifyExerciseDTO exerciseDTO)
        {
            if (string.IsNullOrEmpty(exerciseID)) // field validate
                throw new ArgumentException($"El ID del ejercicio no puede estar vacío. {nameof(exerciseID)}");
            if (!await ValidateExerciseID(exerciseID)) // field validate
                throw new ArgumentException($"El ejercicio con ID: {exerciseID} no existe.");
            try
            {
                // create update definitions
                var updateBuilder = Builders<ExercisesModel>.Update;
                var updateDefinitions = new List<UpdateDefinition<ExercisesModel>>();

                // modify not null field
                if (!string.IsNullOrEmpty(exerciseDTO.Photo)) // photo
                    updateDefinitions.Add(updateBuilder.Set(exercise => exercise.Photo, exerciseDTO.Photo));
                if (!string.IsNullOrEmpty(exerciseDTO.Name)) // name
                {
                    if (await ValidateExerciseName(exerciseDTO.Name)) // Verifica si el nombre ya está en uso
                        throw new ArgumentException($"El nombre del ejercicio: {exerciseDTO.Name} ya está en uso.");

                    updateDefinitions.Add(updateBuilder.Set(exercise => exercise.Name, exerciseDTO.Name));
                }
                if (!string.IsNullOrEmpty(exerciseDTO.Description)) // description
                    updateDefinitions.Add(updateBuilder.Set(exercise => exercise.Description, exerciseDTO.Description));
                if (exerciseDTO.Series.HasValue && exerciseDTO.Series > 0) // series
                    updateDefinitions.Add(updateBuilder.Set(exercise => exercise.Series, exerciseDTO.Series));
                if (exerciseDTO.Repetitions.HasValue && exerciseDTO.Series > 0) // repitions
                    updateDefinitions.Add(updateBuilder.Set(exercise => exercise.Repetitions, exerciseDTO.Repetitions));

                // verification
                if (!updateDefinitions.Any())
                    throw new Exception("No se proporcionaron campos válidos para modificar.");

                // combine to a single
                var combine = updateBuilder.Combine(updateDefinitions);

                // modify
                var filter = Builders<ExercisesModel>
                    .Filter
                    .Eq(exercise => exercise.Exercise_ID, exerciseID);
                // result of the modify
                var update = await _exercisesCollection
                    .UpdateOneAsync(filter, combine);
                // check if the update was successful
                if (update.ModifiedCount == 0)
                    throw new ArgumentException("Error al modificar ejercicio.");

                // exercise
                ExercisesModel exercises = await _exercisesCollection
                    .Find(exercise => exercise.Exercise_ID == exerciseID)
                    .Project<ExercisesModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // exercise
                return exercises;
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
        public async Task<ExercisesModel> DeleteExercises(string exerciseID)
        {
            if (string.IsNullOrEmpty(exerciseID)) // field verification
                throw new ArgumentException("El ID del ejercicio no puede estar vacío.", nameof(exerciseID));
            if (!await ValidateExerciseID(exerciseID)) // field verification
                throw new ArgumentException($"El ejercicio con ID: {exerciseID} no existe.");
            try
            {
                // exercise
                var exercise = await _exercisesCollection
                    .Find(exercise => exercise.Exercise_ID == exerciseID)
                    .FirstOrDefaultAsync();

                // check if is not null
                if (exercise == null)
                    throw new ArgumentException("Ejercicio no encontrado.");

                // delete
                await _exercisesCollection.DeleteOneAsync(exercise => exercise.Exercise_ID == exerciseID);

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
