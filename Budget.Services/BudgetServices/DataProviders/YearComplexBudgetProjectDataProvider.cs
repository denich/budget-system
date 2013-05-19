﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Budget.Services.BudgetModel;
using Budget.Services.BudgetServices.DataProviderContracts;
using Budget.Services.Helpers;
using Microsoft.Practices.Unity;
using MoreLinq;

namespace Budget.Services.BudgetServices.DataProviders
{
    public class YearComplexBudgetProjectDataProvider : BaseComplexBudgetProjectDataProvider, IYearComplexBudgetProjectDataProvider 
    {
        private readonly CustomDataProvider<YearComplexBudgetProject> _provider;

        [Dependency]
        public IQuarterComplexBudgetProjectDataProvider QuarterComplexBudgetProjectDataProvider { get; set; }

        [InjectionConstructor]
        public YearComplexBudgetProjectDataProvider([Dependency("ConnectionString")] string connectionString,
                                                    [Dependency("YearComplexBudgetProjectProcedures")] DbProcedureSet
                                                        procedureSet)
        {
            _provider = new CustomDataProvider<YearComplexBudgetProject>(connectionString, procedureSet);
        }



        public IEnumerable<YearComplexBudgetProject> GetAll()
        {
            return _provider.GetItems();
        }

        public YearComplexBudgetProject Get(int id)
        {
            return _provider.GetItem(id);
        }

        public int Insert(YearComplexBudgetProject yearComplexBudgetProject)
        {
            yearComplexBudgetProject.Revision = GetBudgetNextRevision(yearComplexBudgetProject.Year,
                                                                      yearComplexBudgetProject.AdministrativeUnitId);
            yearComplexBudgetProject.RevisionDate = DateTime.Now;

            int yearBudgetId = _provider.AddItem(yearComplexBudgetProject);

            InsertCategoriesRecursivly(yearComplexBudgetProject.BudgetCategories, yearBudgetId);

            //Insert quarter budgets
            var quarterBudgetNumberIds = new Dictionary<int, int>();

            foreach (var quarterBudget in yearComplexBudgetProject.QuarterBudgets)
            {
                quarterBudget.YearBudgetID = yearBudgetId;

                int quarterBudgetId = QuarterComplexBudgetProjectDataProvider.Insert(quarterBudget);

                quarterBudgetNumberIds.Add(quarterBudget.QuarterNumber, quarterBudgetId);
            }
            
            if (!yearComplexBudgetProject.ChildBudgets.Any())
            {
                return yearBudgetId;
            }

            //Add master quarter budgets to childs
            if (yearComplexBudgetProject.HasQuarterBudgets)
            {
                foreach (var quarterBudgetId in quarterBudgetNumberIds)
                {
                    yearComplexBudgetProject.ChildBudgets = yearComplexBudgetProject.ChildBudgets.Select(b =>
                    {
                        b.QuarterBudgets.Single(q => q.QuarterNumber == quarterBudgetId.Key).MasterBudgetID = quarterBudgetId.Value;
                        return b;
                    }).ToList();   
                }
            }

            //Add master month budgets to childs
            if (yearComplexBudgetProject.HasQuarterMonthBudgets)
            {
                Dictionary<int, int> monthBudgetsIds =
                    quarterBudgetNumberIds.SelectMany(
                        q => QuarterComplexBudgetProjectDataProvider.Get(q.Value).MonthBudgets)
                                          .ToDictionary(b => b.Month, b => b.Id);

                yearComplexBudgetProject.ChildBudgets = yearComplexBudgetProject.ChildBudgets.Select(c =>
                    {
                        c.QuarterBudgets = c.QuarterBudgets.Select(
                            q =>
                                {
                                    q.MonthBudgets = q.MonthBudgets.Select(m =>
                                        {
                                            m.MasterBudgetID = monthBudgetsIds[m.Month];
                                            return m;
                                        }).ToList();
                                    return q;
                                }).ToList();
                        return c;
                    }).ToList();

            }

            //Insert child budgets
            foreach (var childBudget in yearComplexBudgetProject.ChildBudgets)
            {
                childBudget.MasterBudgetID = yearBudgetId;
                Insert(childBudget);
            }
            
            return yearBudgetId;
        }

        public int Update(YearComplexBudgetProject yearComplexBudgetProject)
        {
            return _provider.UpdateItem(yearComplexBudgetProject);
        }

        public int Delete(int id)
        {
            return _provider.DeleteItem(id);
        }

        public IEnumerable<YearComplexBudgetProject> GetBudgetProjects(int year, int fcenterId)
        {
            return GetAll().Where(b => b.AdministrativeUnitId == fcenterId && b.Year == year);
        }

        public YearComplexBudgetProject GetLatestAcceptedBudgetProject(int year, int fcenterId)
        {
            return
                GetAll()
                    .Where(b => b.AdministrativeUnitId == fcenterId && b.Year == year && b.IsAccepted)
                    .MinBy(p => p.Id);
        }

        public YearComplexBudgetProject GetFinalFor(int adminUnitId, int year)
        {
            return GetAll().FirstOrDefault(b => b.AdministrativeUnitId == adminUnitId && b.Year == year && b.IsFinal);
        }

        public IEnumerable<UnapproveYearBudget> GetUnapprovalBudgets(int adminUnitId)
        {
            var yearComplexBudgetProjects = _provider.GetItems().Where(b => b.AdministrativeUnitId == adminUnitId && !b.IsFinal).ToList();

            return
                yearComplexBudgetProjects
                         .GroupBy(b => b.Year, (key, group) => new UnapproveYearBudget
                             {
                                 LastApprovedBudgetId = group.Where(b => b.IsAccepted).MaxBy(b => b.Revision).Id,
                                 Year = key,
                                 RevisionCount = group.Count(),
                                 WaitingOfferCount = group.Count(x => x.Status == BudgetProjectStatus.Waiting)
                             });
        }

        public IEnumerable<YearComplexBudgetProject> GetByMaster(int masterBudgetId)
        {
            return GetAll().Where(b => b.MasterBudgetID == masterBudgetId);
        }

        public YearComplexBudgetProject GetTemplate()
        {
            return IocContainer.Instance.Resolve<YearComplexBudgetProject>();
        }

        private int GetBudgetNextRevision(int year, int fcenterId)
        {
            var fcenterBudgets = GetBudgetProjects(year, fcenterId);

            return fcenterBudgets.Any() ? fcenterBudgets.Max(p => p.Revision) + 1 : 0;
        }
    }
}
