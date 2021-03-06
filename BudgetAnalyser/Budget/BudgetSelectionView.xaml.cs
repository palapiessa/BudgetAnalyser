﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BudgetAnalyser.Budget
{
    /// <summary>
    ///     Interaction logic for BudgetSelectionView.xaml
    /// </summary>
    public partial class BudgetSelectionView : UserControl
    {
        public BudgetSelectionView()
        {
            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var budgetController = e.NewValue as BudgetController;
                if (budgetController != null)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(budgetController.Budgets);
                    view.SortDescriptions.Add(new SortDescription("EffectiveFrom", ListSortDirection.Descending));
                }
            }
        }
    }
}