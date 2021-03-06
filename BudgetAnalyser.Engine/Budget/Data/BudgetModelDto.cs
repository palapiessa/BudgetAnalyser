﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Budget.Data
{
    public class BudgetModelDto
    {
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        ///     No need for a data type for <see cref="Income" />, <see cref="Expenses" />, <see cref="BudgetItem" />,
        ///     as these have no properties that need to be private.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Necessary for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Necessary for serialisation")]
        public List<ExpenseDto> Expenses { get; set; }

        /// <summary>
        ///     No need for a data type for <see cref="Income" />, <see cref="Expenses" />, <see cref="BudgetItem" />,
        ///     as these have no properties that need to be private.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Necessary for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Necessary for serialisation")]
        public List<IncomeDto> Incomes { get; set; }

        /// <summary>
        ///     Gets the date and time the budget model was last modified by the user.
        /// </summary>
        public DateTime? LastModified { get; set; }

        /// <summary>
        ///     Gets an optional comment than can be given when a change is made to the budget model.
        /// </summary>
        public string LastModifiedComment { get; set; }

        public string Name { get; set; }
    }
}