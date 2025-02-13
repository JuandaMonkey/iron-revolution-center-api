using iron_revolution_center_api.DTOs.Exercise;
using iron_revolution_center_api.DTOs.Staff;
using iron_revolution_center_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data.Interface
{
    public interface iExercisesService
    {
        // list of exercises
        public Task<IEnumerable<ExercisesModel>> ListExercises();

        // register exercises
        public Task<InsertExerciseDTO> RegisterExercises(InsertExerciseDTO exerciseDTO);

        // modify exerecises
        public Task<ExercisesModel> ModifyExercises(string exerciseID, ModifyExerciseDTO exerciseDTO);

        // delete exercises
        public Task<ExercisesModel> DeleteExercises(string exerciseID);
    }
}
