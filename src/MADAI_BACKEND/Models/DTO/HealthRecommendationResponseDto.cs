using System;
using System.ComponentModel.DataAnnotations;

namespace MADAI_BACKEND.Models.DTO
{
    public class HealthRecommendationResponseDto
    {
       
        public string Recommendations { get; set; } = string.Empty;
    }
}
