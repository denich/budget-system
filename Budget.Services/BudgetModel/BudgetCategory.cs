﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Budget.Services.BudgetServices.DataProviderContracts;
using Budget.Services.BudgetServices.DataProviders;
using Budget.Services.Helpers;
using Microsoft.Practices.Unity;

namespace Budget.Services.BudgetModel
{
    public class BudgetCategory : IDataRetriever<BudgetCategory>
    {
        private IEnumerable<TargetBudget> _targetBudgets;

        private Employe _responsibleEmploye;

        private BudgetCategoryInfo _info;

        [Dependency]
        public ITargetBudgetDataProvider BudgetCategoryDataProvider { get; set; }

        [Dependency]
        public IEmployeDataProvider EmployeDataProvider { get; set; }

        [Dependency]
        public IBudgetCategoryInfoDataProvider BudgetCategoryInfoDataProvider { get; set; }

        public int Id { get; set; }

        public double Value { get; set; }

        public int ComplexBudgetId { get; set; }

        public int InfoId { get; set; }

        public BudgetCategoryInfo Info
        {
            get { return _info ?? BudgetCategoryInfoDataProvider.Get(InfoId); }
            set
            {
                _info = value;

                InfoId = value.Id;
            }
        }

        public int ResponsibleEmployeId { get; set; }

        public Employe ResponsibleEmploye
        {
            get { return _responsibleEmploye ?? EmployeDataProvider.Get(ResponsibleEmployeId); }
            set
            {
                _responsibleEmploye = value;

                ResponsibleEmployeId = value.Id;
            }
        }

        public IEnumerable<TargetBudget> TargetBudgets
        {
            get
            {
                if (_targetBudgets != null)
                {
                    return _targetBudgets;
                }

                var targetBudgets = BudgetCategoryDataProvider.GetAll();

                return targetBudgets == null ? null : targetBudgets.Where(t => t.BudgetCategoryId == Id);
            }
            set { _targetBudgets = value; }
        }

        public ICollection<SqlParameter> InsertSqlParameters
        {
            get
            {
                return new[]
                    {
                        new SqlParameter("InfoId", SqlHelper.GetSqlValue(InfoId)),
                        new SqlParameter("Value", SqlHelper.GetSqlValue(Value)),
                        new SqlParameter("ResponsibleEmployeId", SqlHelper.GetSqlValue(ResponsibleEmployeId)),
                        new SqlParameter("ComplexBudgetId", SqlHelper.GetSqlValue(ComplexBudgetId)),
                    };
            }
        }

        public ICollection<SqlParameter> UpdateSqlParameters
        {
            get
            {
                var sqlParams = InsertSqlParameters;
                InsertSqlParameters.Add(new SqlParameter("Id", Id));
                return sqlParams;
            }
        }

        public BudgetCategory Setup(IDataRecord record)
        {
            Id = Convert.ToInt32(record["Id"]);
            InfoId = Convert.ToInt32(record["InfoId"]);
            Value = Convert.ToDouble(record["Value"]);
            ResponsibleEmployeId = Convert.ToInt32(record["ResponsibleEmployeId"]);
            ComplexBudgetId = Convert.ToInt32(record["ComplexBudgetId"]);
            return this;
        }
    }
}
