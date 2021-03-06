﻿using CourtCaseManagement.ApplicationCore.Interfaces;
using System.Collections.Generic;

namespace CourtCaseManagement.ApplicationCore.Entities
{
    public class SituationEntity : BaseEntity, IAggregateRoot
    {
        public virtual string Name { get; set; }
        public virtual bool? Finished { get; set; }
        public virtual IList<ProcessEntity> Processes { get; set; }
    }
}