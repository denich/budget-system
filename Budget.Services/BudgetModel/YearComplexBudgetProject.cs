﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Budget.Services.BudgetServices.DataProviderContracts;
using Budget.Services.BudgetServices.DataProviders;
using Budget.Services.Helpers;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;

namespace Budget.Services.BudgetModel
{
    public class YearComplexBudgetProject : YearComplexBudget, IDataRetriever<YearComplexBudgetProject>
    {
        [Dependency]
        public IQuarterComplexBudgetProjectDataProvider QuarterComplexBudgetProjectDataProvider { get; set; }

        [Dependency]
        public IYearComplexBudgetProjectDataProvider YearComplexBudgetProjectDataProvider { get; set; }

        private readonly BudgetProject _budgetProject = new BudgetProject();

        private IEnumerable<YearComplexBudgetProject> _childBudgets;

        private IEnumerable<QuarterComplexBudgetProject> _quarterBudgets;

        public int UpdatedPersonId
        {
            get { return _budgetProject.UpdatedPersonId; }
            set { _budgetProject.UpdatedPersonId = value; }
        }

        public int Revision
        {
            get { return _budgetProject.Revision; }
            set { _budgetProject.Revision = value; }
        }

        public DateTime RevisionDate
        {
            get { return _budgetProject.RevisionDate; }
            set { _budgetProject.RevisionDate = value; }
        }

        public Employe UpdatedPerson
        {
            get { return _budgetProject.UpdatedPerson; }
            set { _budgetProject.UpdatedPerson = value; }
        }

        public bool IsAccepted
        {
            get { return _budgetProject.IsAccepted; }
        }

        public BudgetProjectStatus Status
        {
            get { return _budgetProject.Status; }
            set { _budgetProject.Status = value; }
        }

        public string Comment
        {
            get { return _budgetProject.Comment; }
            set { _budgetProject.Comment = value; }
        }

        public IEnumerable<YearComplexBudgetProject> ChildBudgets
        {
            get
            {
                return _childBudgets ?? YearComplexBudgetProjectDataProvider.GetByMaster(Id);
            }
            set
            {
                _childBudgets = value;
            }
        }

        public IEnumerable<QuarterComplexBudgetProject> QuarterBudgets
        {
            get { return _quarterBudgets ?? QuarterComplexBudgetProjectDataProvider.GetChildForYearBudget(Id); }
            set { _quarterBudgets = value; }
        }

        public bool HasQuarterBudgets
        {
            get { return QuarterBudgets.Any(); }
        }

        public bool HasQuarterMonthBudgets
        {
            get { return QuarterBudgets.Any() && QuarterBudgets.First().MonthBudgets.Any(); }
        }

        public override ICollection<SqlParameter> InsertSqlParameters
        {
            get
            {
                var sqlParams = new List<SqlParameter>(_budgetProject.SqlParameters);

                sqlParams.AddRange(base.InsertSqlParameters);

                return sqlParams;
            }
        }

        public override ICollection<SqlParameter> UpdateSqlParameters
        {
            get { return InsertSqlParameters; }
        }

        public new YearComplexBudgetProject Setup(IDataRecord record)
        {
            _budgetProject.Setup(record);
            base.Setup(record);
            return this;
        }

        public void GenerateQuarterBudgets()
        {
            GenerateQuarterBudgets(false);
        }

        public void GenerateQuarterMonthBudgets()
        {
            GenerateQuarterBudgets(true);
        }

        public override void CalculateValues()
        {
            if (ChildBudgets != null && ChildBudgets.Any())
            {
                ChildBudgets.ForEach(b => b.CalculateValues());

                BudgetCategories = GetValuesSumFormCategories(BudgetCategories, ChildBudgets.SelectMany(b => b.BudgetCategories));

                return;
            }

            if (QuarterBudgets != null && QuarterBudgets.Any())
            {
                QuarterBudgets.ForEach(b => b.CalculateValues());
                BudgetCategories = GetValuesSumFormCategories(BudgetCategories, QuarterBudgets.SelectMany(b => b.BudgetCategories));
                return;
            }

            base.CalculateValues();
        }

        private void GenerateQuarterBudgets(bool includeMonthBudgets)
        {
            var quarterBudgets = new List<QuarterComplexBudgetProject>();

            for (int quarterNumber = 1; quarterNumber <= 4; quarterNumber++)
            {
                quarterBudgets.Add(GetQuarterBudget(quarterNumber, includeMonthBudgets));
            }

            QuarterBudgets = quarterBudgets;
        }

        private QuarterComplexBudgetProject GetQuarterBudget(int quarterNumber, bool includeMonthBudgets)
        {

            var quarterBudget = QuarterComplexBudgetProjectDataProvider.GetTemplate();
            quarterBudget.Year = Year;
            quarterBudget.QuarterNumber = quarterNumber;
            quarterBudget.AdministrativeUnitId = AdministrativeUnitId;
            quarterBudget.UpdatedPersonId = UpdatedPersonId;
            quarterBudget.Status = Status;
            quarterBudget.BudgetCategories = BudgetCategories != null
                                                 ? BudgetCategories.Select(c => c.ClearValues())
                                                 : null;
                

            if (includeMonthBudgets)
            {
                quarterBudget.GenerateMonthBudgets();
            }

            return quarterBudget;
        }
    }
}