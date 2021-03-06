﻿using CourtCaseManagement.ApplicationCore.Entities;
using CourtCaseManagement.ApplicationCore.Interfaces;

namespace CourtCaseManagement.Infrastructure.Data
{
    public class ResponsibleRepository : EfRepository<ResponsibleEntity>, IResponsibleRepository
    {
        public ResponsibleRepository(CatalogContext dbContext) : base(dbContext)
        {

        }
    }
}