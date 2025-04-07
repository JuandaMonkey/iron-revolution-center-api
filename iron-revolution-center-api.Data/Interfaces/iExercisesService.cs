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
    // interface for 
    public interface iExercisesService
    {
        // list of exercises
        public Task<IEnumerable<ExercisesModel>> ListExercises(string exerciseType);

        // register exercises
        public Task<InsertExerciseDTO> InsertExercises(newExerciseDTO exerciseDTO);

        // modify exerecises
        public Task<ExercisesModel> ModifyExercises(string exerciseId, ModifyExerciseDTO exerciseDTO);

        // delete exercises
        public Task<ExercisesModel> DeleteExercises(string exerciseId);
    }
}
