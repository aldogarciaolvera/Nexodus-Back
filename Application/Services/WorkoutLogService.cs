using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services
{
    public class WorkoutLogService
    {
        private readonly IWorkoutLogRepository _workoutLogRepository;
        private readonly IRoutineRepository _routineRepository;

        public WorkoutLogService(IWorkoutLogRepository workoutLogRepository, IRoutineRepository routineRepository)
        {
            _workoutLogRepository = workoutLogRepository;
            _routineRepository = routineRepository;
        }

        public async Task<IEnumerable<WorkoutLogDto>> GetAllByUserIdAsync(Guid userId)
        {
            var logs = await _workoutLogRepository.GetAllByUserIdAsync(userId);
            return logs.Select(MapToDto);
        }

        public async Task<WorkoutLogDto> GetByIdAsync(Guid id, Guid userId)
        {
            var log = await _workoutLogRepository.GetByIdAsync(id);

            if (log == null || log.UserId != userId)
            {
                throw new NotFoundException($"No se encontró un registro de entrenamiento con ID {id}.");
            }

            return MapToDto(log);
        }

        public async Task<WorkoutLogDto> CreateAsync(CreateWorkoutLogDto dto, Guid userId)
        {
            var routine = await _routineRepository.GetByIdAsync(dto.RoutineId, userId);
            if (routine == null)
            {
                throw new NotFoundException($"No se encontró una rutina con ID {dto.RoutineId} para asociar al registro.");
            }

            var log = new WorkoutLog
            {
                UserId = userId,
                RoutineId = dto.RoutineId,
                DurationInSeconds = dto.DurationInSeconds,
                CompletedExercisesCount = dto.CompletedExercisesCount,
                DateCompleted = dto.DateCompleted,
            };

            await _workoutLogRepository.AddAsync(log);

            return MapToDto(log);
        }

        public async Task DeleteAsync(Guid id, Guid userId)
        {
            var log = await _workoutLogRepository.GetByIdAsync(id);

            if (log == null || log.UserId != userId)
            {
                throw new NotFoundException($"No se encontró un registro de entrenamiento con ID {id}.");
            }

            await _workoutLogRepository.DeleteAsync(log);
        }

        private static WorkoutLogDto MapToDto(WorkoutLog log)
        {
            return new WorkoutLogDto
            {
                Id = log.Id,
                RoutineId = log.RoutineId,
                DurationInSeconds = log.DurationInSeconds,
                CompletedExercisesCount = log.CompletedExercisesCount,
                DateCompleted = log.DateCompleted,
                CreatedAt = log.CreatedAt
            };
        }
    }
}
