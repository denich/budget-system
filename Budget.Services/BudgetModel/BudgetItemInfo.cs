﻿using System;

namespace Budget.Services.BudgetModel
{
    public class BudgetItemInfo
    {
        public int Id { get; set; }

        public int TargetBudgetId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime DateAdded { get; set; }

        public string Source { get; set; }
    }
}
