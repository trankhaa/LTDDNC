using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities.Doctor;

public class DoctorSchedule
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string IdDoctorSchedule { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("doctorId")]
    public string DoctorId { get; set; } = string.Empty;

    [BsonElement("consultationFee")]
    public int ConsultationFee { get; set; }

    [BsonElement("startTime")]
    public string StartTime { get; set; } = string.Empty; // format "HH:mm"

    [BsonElement("endTime")]
    public string EndTime { get; set; } = string.Empty; // format "HH:mm"

    [BsonElement("examinationTime")]
    public int ExaminationTime { get; set; } // minutes
}