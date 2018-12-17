﻿using FitnessApp.Models;
using FitnessApp.SQLdatabase;
using FitnessApp.ViewModels;
using FitnessApp.Windows;
using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace FitnessApp.UserWindowPages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        SQLqueries SQLqueriesObject = new SQLqueries();

        public HomePage()
        {
            InitializeComponent();
            UserWindow.HomePageObject = this;

            LoadWeightChart();
            LoadTotalWeightLostCard();
            LoadAverageWeightLostCard();
            LoadJoinedChallengesCards();
            LoadJoinedPlanCard();
            LoadMotivationalQuoteCard();
            LoadCaloriesCard();

            FoodComboBox.ItemsSource = SQLqueriesObject.GetAllFood();
            WorkoutsComboBox.ItemsSource = SQLqueriesObject.GetAllWorkouts();

        }


        ////////// All Weight Cards Functions/Event Handlers //////////

        // Weight Chart Properties

        public SeriesCollection WeightsSeriesCollection { get; set; }
        public List<string> Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public void LoadWeightChart()
        {

            WeightsSeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Ideal Weight",
                    Values = Enumerable.Repeat(CalculateIdealWeight(), 10).AsChartValues(),
                    PointGeometry = null,
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.ForestGreen,
                    StrokeDashArray = new DoubleCollection {3},
                },

                new LineSeries
                {
                    Title = "Weight",
                    Values = SQLqueriesObject.GetWeightValues(UserWindow.signedInUser.ID).AsChartValues(),
                },

                new LineSeries
                {
                    Title = "Target Weight",
                    Values = Enumerable.Repeat(UserWindow.signedInUser.TargetWeight, 10).AsChartValues(),
                    PointGeometry = null,
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Red,
                    StrokeDashArray = new DoubleCollection {3},
                }
                
            };

            Labels = SQLqueriesObject.GetWeightDateValues(UserWindow.signedInUser.ID);
            YFormatter = value => value.ToString() + " kg";

            // Setting Data context for Weight Chart
            WeightChart.DataContext = this;
        }

        private double CalculateIdealWeight()
        {
            if (UserWindow.signedInUser.Gender == "Male")
                return (UserWindow.signedInUser.Height - 100) + ((UserWindow.signedInUser.Height - 100) * 0.10);

            else
                return (UserWindow.signedInUser.Height - 100) + ((UserWindow.signedInUser.Height - 100) * 0.15);
        }

        private void DecimalNumbersOnlyFieldValidation(object sender, TextCompositionEventArgs e)
        {
            var s = sender as TextBox;
            var text = s.Text.Insert(s.SelectionStart, e.Text);
            e.Handled = !double.TryParse(text, out double d);
        }

        private void SaveWeightButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(TodaysWeightTextBox.Text))
                UserWindow.UserWindowObject.MessagesSnackbar.MessageQueue.Enqueue("Please enter your weight!");
            else
            {
                // Update User Model Weight Porperty with the latest weight
                UserWindow.signedInUser.Weight = double.Parse(TodaysWeightTextBox.Text);

                // Update Weight in Database
                SQLqueriesObject.AddNewWeight(double.Parse(TodaysWeightTextBox.Text), UserWindow.signedInUser.ID);

                // Update User Weight Line Series:
                // Add one value and remove another to keep the number of values 10
                WeightChart.Series[1].Values.Add(double.Parse(TodaysWeightTextBox.Text));
                if (WeightChart.Series[1].Values.Count > 10)
                    WeightChart.Series[1].Values.RemoveAt(0);

                // Confirmation Message
                UserWindow.UserWindowObject.MessagesSnackbar.MessageQueue.Enqueue("Weight added successfully");

                // Reset TextBox
                TodaysWeightTextBox.Text = "";

                // Refresh Weight-Related Cards
                LoadTotalWeightLostCard();
                LoadAverageWeightLostCard();

                // Refresh Calories Card and Chart
                CaloriesChart.DataContext = null;
                LoadCaloriesCard();

                // Refresh CaloriesCalculatorPage DataContext
                UserWindow.CaloriesCalculatorPageObject.DataContext = null;
                UserWindow.CaloriesCalculatorPageObject.DataContext = UserWindow.signedInUser;

                // Refresh SettingsPage DataContext
                UserWindow.SettingsPageObject.DataContext = null;
                UserWindow.SettingsPageObject.DataContext = UserWindow.signedInUser;
            }
        }

        public void LoadTotalWeightLostCard()
        {

            double WeightLostPerWeek = SQLqueriesObject.GetTotalWeightLostPerWeek(UserWindow.signedInUser.ID);
            double WeightLostPerMonth = SQLqueriesObject.GetTotalWeightLostPerMonth(UserWindow.signedInUser.ID);
            double WeightLostPerYear = SQLqueriesObject.GetTotalWeightLostPerYear(UserWindow.signedInUser.ID);


            // Set Colours
            if (WeightLostPerWeek < 0)
                WeightLostPerWeekTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            else
                WeightLostPerWeekTextBlock.Foreground = (Brush)Application.Current.Resources["PrimaryHueMidBrush"];

            if (WeightLostPerWeek < 0)
                WeightLostPerMonthTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            else
                WeightLostPerMonthTextBlock.Foreground = (Brush)Application.Current.Resources["PrimaryHueMidBrush"];

            if (WeightLostPerWeek < 0)
                WeightLostPerYearTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            else
                WeightLostPerYearTextBlock.Foreground = (Brush)Application.Current.Resources["PrimaryHueMidBrush"];


            // Assign Values to TextBoxes
            WeightLostPerWeekTextBlock.Text = WeightLostPerWeek.ToString();
            WeightLostPerMonthTextBlock.Text = WeightLostPerMonth.ToString();
            WeightLostPerYearTextBlock.Text = WeightLostPerYear.ToString();

        }

        public void LoadAverageWeightLostCard()
        {

            double AverageWeightLostPerWeek = SQLqueriesObject.GetAverageWeightLostPerWeek(UserWindow.signedInUser.ID);
            double AverageWeightLostPerMonth = SQLqueriesObject.GetAverageWeightLostPerMonth(UserWindow.signedInUser.ID);
            double AverageWeightLostPerYear = SQLqueriesObject.GetAverageWeightLostPerYear(UserWindow.signedInUser.ID);


            // Set Colours
            if (AverageWeightLostPerWeek < 0)
                AverageWeightLostPerWeekTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            else
                AverageWeightLostPerWeekTextBlock.Foreground = (Brush)Application.Current.Resources["PrimaryHueMidBrush"];

            if (AverageWeightLostPerMonth < 0)
                AverageWeightLostPerMonthTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            else
                AverageWeightLostPerMonthTextBlock.Foreground = (Brush)Application.Current.Resources["PrimaryHueMidBrush"];

            if (AverageWeightLostPerYear < 0)
                AverageWeightLostPerYearTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            else
                AverageWeightLostPerYearTextBlock.Foreground = (Brush)Application.Current.Resources["PrimaryHueMidBrush"];


            // Assign Values to TextBoxes
            AverageWeightLostPerWeekTextBlock.Text = AverageWeightLostPerWeek.ToString();
            AverageWeightLostPerMonthTextBlock.Text = AverageWeightLostPerMonth.ToString();
            AverageWeightLostPerYearTextBlock.Text = AverageWeightLostPerYear.ToString();
        }




        ////////// Joined Challenges Cards Functions/Event Handlers //////////


        // Setting Data context for JoinedChallengesListBox
        public void LoadJoinedChallengesCards()
        {
            ChallengesViewModel joinedChallengesDataContext = new ChallengesViewModel();
            joinedChallengesDataContext.JoinedChallengesViewModel(UserWindow.signedInUser.ID);
            CompletedJoinedChallengesListBox.DataContext = joinedChallengesDataContext;

            ChallengesViewModel uncompletedJoinedChallengesDataContext = new ChallengesViewModel();
            uncompletedJoinedChallengesDataContext.JoinedChallengesViewModel(UserWindow.signedInUser.ID);
            UncompletedJoinedChallengesListBox.DataContext = uncompletedJoinedChallengesDataContext;
            ControlNoChallengesCard(joinedChallengesDataContext);
        }

        private void JoinChallengeButton_Click(object sender, RoutedEventArgs e)
        {
            UserWindow.UserWindowObject.UserWindowPagesListBox.SelectedIndex = 1;
        }

        private void UnjoinChallengeButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            int selectedChallengeIndex = UncompletedJoinedChallengesListBox.Items.IndexOf(toggleButton.DataContext);

            ChallengeModel currentChallenge = (ChallengeModel)UncompletedJoinedChallengesListBox.Items[selectedChallengeIndex];

            SQLqueriesObject.UnjoinChallenge(UserWindow.signedInUser.ID, currentChallenge.ID);

            // Reloading Data context for JoinedChallengesListBox
            ChallengesViewModel joinedChallengesDataContext = new ChallengesViewModel();
            joinedChallengesDataContext.JoinedChallengesViewModel(UserWindow.signedInUser.ID);
            UncompletedJoinedChallengesListBox.DataContext = joinedChallengesDataContext;

            // Refresh Challenges Page
            UserWindow.ChallengesPageObject.LoadAllChallengesCards();
        }

        private void ControlNoChallengesCard(ChallengesViewModel challengesViewModel)
        {
            if (challengesViewModel.UncompletedJoinedChallengeModels.Count > 0)
                NoChallengesCard.Visibility = Visibility.Collapsed;
            else
                NoChallengesCard.Visibility = Visibility.Visible;
        }

        private void CompletedChallengeButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int selectedChallengeIndex = CompletedJoinedChallengesListBox.Items.IndexOf(button.DataContext);

            ChallengeModel currentChallenge = (ChallengeModel)CompletedJoinedChallengesListBox.Items[selectedChallengeIndex];

            SQLqueriesObject.UnjoinChallenge(UserWindow.signedInUser.ID, currentChallenge.ID);

            LoadJoinedChallengesCards();
        }



        ////////// Joined Plan Card Functions/Event Handlers //////////


        public void LoadJoinedPlanCard()
        {
            bool checkJoinedInPlan = SQLqueriesObject.IsInPlan(UserWindow.signedInUser.ID);

            NoPlanCard.Visibility = Visibility.Visible;
            JoinedPlanCard.Visibility = Visibility.Visible;
            PlanCompletedCard.Visibility = Visibility.Visible;

            if (checkJoinedInPlan)
            {
                NoPlanCard.Visibility = Visibility.Collapsed;

                int planDayNum = SQLqueriesObject.GetJoinedPlanDayNumber(UserWindow.signedInUser.ID);

                if (planDayNum > 30)
                    JoinedPlanCard.Visibility = Visibility.Collapsed;
                else
                {
                    PlanCompletedCard.Visibility = Visibility.Collapsed;

                    // Load Header
                    string planName = SQLqueriesObject.GetJoinedPlanName(UserWindow.signedInUser.ID).ToString();
                    PlanHeaderTextBlock.Text = planName + " | Day #" + planDayNum;
                    SQLqueriesObject.UpdatePlanDayNumber(UserWindow.signedInUser.ID, planDayNum);

                    // Load CheckBoxes
                    BreakfastCheckBox.IsChecked = SQLqueriesObject.GetDayBreakfastStatus(UserWindow.signedInUser.ID);
                    LunchCheckBox.IsChecked = SQLqueriesObject.GetDayLunchStatus(UserWindow.signedInUser.ID);
                    DinnerCheckBox.IsChecked = SQLqueriesObject.GetDayDinnerStatus(UserWindow.signedInUser.ID);
                    WorkoutsCheckBox.IsChecked = SQLqueriesObject.GetDayWorkoutStatus(UserWindow.signedInUser.ID);

                    // Load Descriptions
                    BreakfastDescriptionTextBlock.Text = SQLqueriesObject.GetDayBreakfastDescription(UserWindow.signedInUser.ID);
                    LunchDescriptionTextBlock.Text = SQLqueriesObject.GetDayLucnchDescription(UserWindow.signedInUser.ID);
                    DinnerDescriptionTextBlock.Text = SQLqueriesObject.GetDayDinnerDescription(UserWindow.signedInUser.ID);
                    WorkoutsDescriptionTextBlock.Text = SQLqueriesObject.GetDayWorkoutDescription(UserWindow.signedInUser.ID);

                    // Load Progress Bar
                    PlanProgressBar.Value = planDayNum;
                }
            }
            else
            {
                JoinedPlanCard.Visibility = Visibility.Collapsed;
                PlanCompletedCard.Visibility = Visibility.Collapsed;
            }
        }

        private void DayItemCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox currentCheckBox = sender as CheckBox;

            switch (currentCheckBox.Name)
            {
                case "BreakfastCheckBox":
                    SQLqueriesObject.UpdateDayBreakfastStatus(true, UserWindow.signedInUser.ID);
                    break;

                case "LunchCheckBox":
                    SQLqueriesObject.UpdateDayLunchStatus(true, UserWindow.signedInUser.ID);
                    break;

                case "DinnerCheckBox":
                    SQLqueriesObject.UpdateDayDinnerStatus(true, UserWindow.signedInUser.ID);
                    break;

                case "WorkoutsCheckBox":
                    SQLqueriesObject.UpdateDayWorkoutStatus(true, UserWindow.signedInUser.ID);
                    break;
            }
        }

        private void DayItemCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox currentCheckBox = sender as CheckBox;

            switch (currentCheckBox.Name)
            {
                case "BreakfastCheckBox":
                    SQLqueriesObject.UpdateDayBreakfastStatus(false, UserWindow.signedInUser.ID);
                    break;

                case "LunchCheckBox":
                    SQLqueriesObject.UpdateDayLunchStatus(false, UserWindow.signedInUser.ID);
                    break;

                case "DinnerCheckBox":
                    SQLqueriesObject.UpdateDayDinnerStatus(false, UserWindow.signedInUser.ID);
                    break;

                case "WorkoutsCheckBox":
                    SQLqueriesObject.UpdateDayWorkoutStatus(false, UserWindow.signedInUser.ID);
                    break;
            }
        }

        private void JoinPlanButton_Click(object sender, RoutedEventArgs e)
        {
            UserWindow.UserWindowObject.UserWindowPagesListBox.SelectedIndex = 2;
        }

        private void DismissPlanButton_Click(object sender, RoutedEventArgs e)
        {
            SQLqueriesObject.UnjoinPlan(UserWindow.signedInUser.ID);
            LoadJoinedPlanCard();

            UserWindow.PlansPageObject.LoadAllPlansCards();
        }


        //////////////////////////////////////////////////////////////




        ////////// Motivational Quotes Card Functions/Event Handlers //////////

        private void LoadMotivationalQuoteCard()
        {
            MotiationalQuoteTextBlock.Text = SQLqueriesObject.GetMotivationalQuote();
        }

        //////////////////////////////////////////////////////////////////////




        ////////// Calories Card Functions/Event Handlers //////////

        public SeriesCollection CaloriesSeriesCollection { get; set; }
        public string[] CaloriesLabels { get; set; }
        public Func<double, string> Formatter { get; set; }

        public void LoadCaloriesCard()
        {

            if (SQLqueriesObject.GetTodayDate() != SQLqueriesObject.GetLastWeightDate(UserWindow.signedInUser.ID))
                SQLqueriesObject.WeightCalc(UserWindow.signedInUser);

            double caloiresGained = SQLqueriesObject.CalroiesGainedToday(UserWindow.signedInUser.ID);
            double caloriesNeeded = SQLqueriesObject.CalroiesNeeded(UserWindow.signedInUser);
            double caloriesLost   = SQLqueriesObject.CalroiesLostToday(UserWindow.signedInUser.ID);

            CaloriesGainedTextBlock.Text = caloiresGained.ToString();
            CaloriesNeededTextBlock.Text = caloriesNeeded.ToString();
            CaloriesLostTextBlock  .Text = caloriesLost.ToString();


            CaloriesSeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title  = "Gaind",
                    Values = new ChartValues<double>() { caloiresGained },
                    Fill   = Brushes.Red,
                },

                new ColumnSeries
                {
                    Title  = "Needed",
                    Values = new ChartValues<double>() { caloriesNeeded },
                    Fill   = (Brush)Application.Current.Resources["PrimaryHueMidBrush"],
                },

                new ColumnSeries
                {
                     Title  = "Lost",
                     Values = new ChartValues<double>() { caloriesLost },
                     Fill   = Brushes.ForestGreen,
                }
            };


            CaloriesLabels = new[] { "Calories" };
            Formatter = value => value.ToString() + "  kCal.";
            CaloriesChart.DataContext = this;

        }


        ///////////////////////////////////////////////////////////




        ////////// PopUpBox Functions/Event Handlers //////////

        private void AddFoodButton_Click(object sender, RoutedEventArgs e)
        {
            DialogBox.IsOpen = true;
            AddFoodDialogBox.Visibility = Visibility.Visible;
        }

        private void AddWorkoutButton_Click(object sender, RoutedEventArgs e)
        {
            DialogBox.IsOpen = true;
            AddWorkoutDialogBox.Visibility = Visibility.Visible;
        }

        //////////////////////////////////////////////////////




        ////////// DialogBoxes Functions/Event Handlers //////////

        private void DialogBoxAddFoodButton_Click(object sender, RoutedEventArgs e)
        {

            if (FoodComboBox.SelectedIndex == -1)
                UserWindow.UserWindowObject.MessagesSnackbar.MessageQueue.Enqueue("Please choose Food!");

            else if (FoodQuantityTextBox.Text == "")
                UserWindow.UserWindowObject.MessagesSnackbar.MessageQueue.Enqueue("Please enter Food Quantity!");

            else
            {
                SQLqueriesObject.AddFood(FoodComboBox.Text, double.Parse(FoodQuantityTextBox.Text), UserWindow.signedInUser.ID);
                AddFoodDialogBox.Visibility = Visibility.Collapsed;
                DialogBox.IsOpen = false;

                // Refresh Calories Card
                CaloriesChart.DataContext = null;
                LoadCaloriesCard();
            }

            // Reset Dialog Box Fields
            FoodComboBox.SelectedIndex = -1;
            FoodQuantityTextBox.Text = "";
        }

        private void DialogBoxAddWorkoutButton_Click(object sender, RoutedEventArgs e)
        {

            if (WorkoutsComboBox.SelectedIndex == -1)
                UserWindow.UserWindowObject.MessagesSnackbar.MessageQueue.Enqueue("Please choose Workout!");

            else if (WorkoutsDurationTextBox.Text == "")
                UserWindow.UserWindowObject.MessagesSnackbar.MessageQueue.Enqueue("Please enter Workout Duration!");

            else
            {
                SQLqueriesObject.AddWorkout(WorkoutsComboBox.Text, double.Parse(WorkoutsDurationTextBox.Text), UserWindow.signedInUser);
                AddWorkoutDialogBox.Visibility = Visibility.Collapsed;
                DialogBox.IsOpen = false;

                // Update Progress of the Challenges having the same type as the entered workout
                SQLqueriesObject.UpdateChallengesProgress(UserWindow.signedInUser.ID, WorkoutsComboBox.Text, double.Parse(WorkoutsDurationTextBox.Text));

                // Refresh Challenges card
                LoadJoinedChallengesCards();

                // Refresh Calories Card
                CaloriesChart.DataContext = null;
                LoadCaloriesCard();
            }

            // Reset Dialog Box Fields
            WorkoutsComboBox.SelectedIndex = -1;
            WorkoutsDurationTextBox.Text = "";
        }

        private void DialogBoxCancelButton_Click(object sender, RoutedEventArgs e)
        {
            AddFoodDialogBox.Visibility = Visibility.Collapsed;
            AddWorkoutDialogBox.Visibility = Visibility.Collapsed;
            DialogBox.IsOpen = false;

            WorkoutsComboBox.SelectedIndex = -1;
            WorkoutsDurationTextBox.Text = "";

            FoodComboBox.SelectedIndex = -1;
            FoodQuantityTextBox.Text = "";
        }

        ///////////////////////////////////////////////////////////


    }
}