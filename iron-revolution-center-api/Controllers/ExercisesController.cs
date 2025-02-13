using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Exercise;
using Microsoft.AspNetCore.Mvc;

namespace iron_revolution_center_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExercisesController : Controller
    {
        private readonly iExercisesService _exercisesService;

        public ExercisesController(iExercisesService exercisesService)
        {
            _exercisesService = exercisesService;
        }

        #region ListExercises
        [HttpGet("List-Exercises")]
        public async Task<IActionResult> ListExercises()
        {
            try
            {
                var exercises = await _exercisesService.ListExercises();

                if (exercises != null)
                    return Ok(exercises);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region RegisterExercises
        [HttpPost("Register-Exercises")]
        public async Task<IActionResult> RegisterExercises([FromBody] InsertExerciseDTO exerciseDTO)
        {
            try
            {
                var exercise = await _exercisesService.RegisterExercises(exerciseDTO);

                if (exercise != null)
                    return Ok(exercise);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region ModifyExercises
        [HttpPut("Modify-Exercises")]
        public async Task<IActionResult> ModifyExercises([FromHeader] string exerciseID, [FromBody] ModifyExerciseDTO exerciseDTO)
        {
            try
            {
                var exercise = await _exercisesService.ModifyExercises(exerciseID, exerciseDTO);
                
                if (exercise != null)
                    return Ok(exercise);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region DeleteExercises
        [HttpDelete("Delete-Exercises")]
        public async Task<IActionResult> DeleteExercises([FromHeader] string exerciseID)
        {
            try
            {
                var exercise = await _exercisesService.DeleteExercises(exerciseID);

                if (exercise != null)
                    return Ok(exercise);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion
    }
}
