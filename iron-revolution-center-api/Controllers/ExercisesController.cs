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
        [HttpGet("Listar-Ejercicios")]
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

        #region InsertExercises
        [HttpPost("Insertar-Ejercicio")]
        public async Task<IActionResult> InsertExercises([FromBody] InsertExerciseDTO exerciseDTO)
        {
            try
            {
                var exercise = await _exercisesService.InsertExercises(exerciseDTO);

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
        [HttpPut("Modificar-Ejercicio")]
        public async Task<IActionResult> ModifyExercises([FromHeader] string exerciseId, [FromBody] ModifyExerciseDTO exerciseDTO)
        {
            try
            {
                var exercise = await _exercisesService.ModifyExercises(exerciseId, exerciseDTO);
                
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
        [HttpDelete("Eliminar-Ejercicio")]
        public async Task<IActionResult> DeleteExercises([FromHeader] string exerciseId)
        {
            try
            {
                var exercise = await _exercisesService.DeleteExercises(exerciseId);

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
