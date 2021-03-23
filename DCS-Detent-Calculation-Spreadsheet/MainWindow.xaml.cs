using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

/*
Welcome to DCS-Detent Calculator Software!
This program will allow you to hit that perfect sweetspot
for your Mil and Afterburner settings every time, all the time.
This software has been designed for every DCS aircraft and every
kind of throttle system or axis. 

JC of DI made the original DCS-Detent Calculator Spreadsheet.
Bailey created the digital representation.
*/

namespace DCS_Detent_Calculation_Spreadsheet
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	/// 

	public partial class MainWindow : Window
    {
		//https://stackoverflow.com/questions/9526626/add-an-array-to-a-dictionary-in-c-sharp
		//Init the dictionary that will contain the aircraft name and their two properties
		Dictionary<string, double[]> dictionary_aircraftAndProperties = new Dictionary<string, double[]>();

		//percent increase of the scale when multiplied by 100
		//this is used to size the drawing
		double plotScale = 2;
		
		Point[] points;//Init the points array
        public MainWindow()
        {
			//===========================================//
			//==!!!!!==Add the new aircraft here==!!!!!==//
			//===========================================//

			//do it like this...
			//dictionary_aircraftAndProperties.Add("AircraftName", new double[] { 'afterburnerLocation' , 'milLocation' });
			dictionary_aircraftAndProperties.Add("AJS37 Viggen", new double[] { 18.0, 19.0 });
			dictionary_aircraftAndProperties.Add("F-16C", new double[] { 20.0, 21.0 });
			dictionary_aircraftAndProperties.Add("F-18C", new double[] { 23.0, 24.0 });
			dictionary_aircraftAndProperties.Add("MiG-29", new double[] { 38.0, 39.0 });
			dictionary_aircraftAndProperties.Add("Su27/Su33", new double[] { 24.0, 25.0 });
			dictionary_aircraftAndProperties.Add("F-14", new double[] { 19.0, 20.0 });
			dictionary_aircraftAndProperties.Add("F-5E", new double[] { 18.0, 19.0 });
			dictionary_aircraftAndProperties.Add("JF-17", new double[] { 8.0, 9.0 });
			dictionary_aircraftAndProperties.Add("M-2000C", new double[] { 11.0, 12.0 });
			dictionary_aircraftAndProperties.Add("Mig-21", new double[] { 8.0, 9.0 });
			dictionary_aircraftAndProperties.Add("F-15C", new double[] { 19.0, 20.0 });
			//dictionary_aircraftAndProperties.Add("Magic Carpet", new double[] { 40.0, 41.0 });//entry test success
			//new aircraft section is finished. Program should be done now. Thanks for updating!

			InitializeComponent();

			//add each dictionary item to the list box. this will result in a list of the aircraft in the list box
			foreach (KeyValuePair<string, double[]> entry in dictionary_aircraftAndProperties)
            {
				listBox1.Items.Add(entry.Key.ToString());
            }

			textBlock_calculationText.Text = "Welcome";//Init the text to something friendly

			//IntegerUpDown_detentLocation.Value = 20;//This will init the detent field
			//i suggest not doing this because it may prevent the user from "knowing what to do next"
			
			////=====Sorts the list of aircraft=====//
			listBox1.Items.SortDescriptions.Add(
			new System.ComponentModel.SortDescription("",
			System.ComponentModel.ListSortDirection.Ascending));

			listBox1.SelectedIndex = 0;//selects the first item in the list. This prevents some null errors
			isAircraftListBoxLoaded = true;//this makes sure that the calculations are not done too early
		}

		//==primary draw formula==//
		private void DrawLine(Point[] points)
		{
			int i;
			int count = points.Length;
			for (i = 0; i < count - 1; i++)
			{
				Line myline = new Line();
				myline.Stroke = Brushes.Blue;
				myline.X1 = points[i].X;
				myline.Y1 = points[i].Y;
				myline.X2 = points[i + 1].X;
				myline.Y2 = points[i + 1].Y;
				canvas.Children.Add(myline);
			}
		}

		//==spare draw formula==//
		private void DrawLine2(Point[] points)
		{
			Polyline line = new Polyline();
			PointCollection collection = new PointCollection();
			foreach (Point p in points)
			{
				collection.Add(p);
			}
			line.Points = collection;
			line.Stroke = new SolidColorBrush(Colors.Black);
			line.StrokeThickness = 1;
			canvas.Children.Add(line);
		}

		private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
				action_aircraftChanged();
		}

        private void action_aircraftChanged()
        {
			//check to see if there is a number in the detent box
			if (IntegerUpDown_detentLocation.Value > 0 && IntegerUpDown_detentLocation.Value < 100)
			{
				//if there is a number, then run calculations to get the updated graph
				//these variables are populated by the stuff in the dictionary, which is
				//selected by what the user selected in the list box
				afterburnerLocation = dictionary_aircraftAndProperties[listBox1.SelectedItem.ToString()][0];
				milLocation = dictionary_aircraftAndProperties[listBox1.SelectedItem.ToString()][1];

				//grab the number that is in the box
				detentLocation = Convert.ToInt32(IntegerUpDown_detentLocation.Value);
				Console.WriteLine("=====NEW CALCULATION for: " + listBox1.SelectedItem + "=====");
				Console.WriteLine("Afterburner Location: " + afterburnerLocation);
				Console.WriteLine("Mil Location: " + milLocation);
				Console.WriteLine("Detent Location: " + detentLocation);
				calculateInputs();
				PrintResultsText();
			}
		}

		//https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/switch
		double afterburnerLocation;
		double milLocation;
		double detentLocation;
		double adSlope;
		double adInt;

		int[] calculationArray = new int[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };//just for using shorthand later
		//https://stackoverflow.com/questions/9569673/accessing-a-dictionary-with-an-array-of-key-values
		double[] resultsLineArray = new double[11];//should contain 11 results from the calculations

		private void calculateInputs()
        {
            //==these are based on given stuff==//
            adSlope = ((100 - milLocation) / (100 - detentLocation));
			adInt = (-((adSlope - 1) * 100));
			//debug math visual
			Console.WriteLine("adSlope is: " +  "(100 - " + milLocation + ") /" + "(100 - " + detentLocation + ") = " + adSlope);
			Console.WriteLine("adInt is: " +"-(" + adSlope + "-1) * 100 = " + adInt);

			//=====the calculation for the result is here==//
			//https://www.google.com/search?q=c%23+clear+array&oq=C%23+clear+ara&aqs=chrome.1.69i57j0i10i22i30j69i58.3439j0j1&sourceid=chrome&ie=UTF-8
			//re-init the array
			Array.Clear(resultsLineArray, 0, 11);
			resultsLineArray = new double[11];

			//this math came from the excel spreadsheet
			for (int i = 0; i < calculationArray.Length; i++)
            {
				if (calculationArray[i] < detentLocation)
				{
					resultsLineArray[i] = Convert.ToInt32(calculationArray[i] * (milLocation / detentLocation));
				}
				else
				{
					resultsLineArray[i] = Convert.ToInt32((calculationArray[i] * adSlope) + adInt);
				}
			}

			//populate the text on the screen
			textBlock_Result00.Text = resultsLineArray[0].ToString();
			textBlock_Result01.Text = resultsLineArray[1].ToString();
			textBlock_Result02.Text = resultsLineArray[2].ToString();
			textBlock_Result03.Text = resultsLineArray[3].ToString();
			textBlock_Result04.Text = resultsLineArray[4].ToString();
			textBlock_Result05.Text = resultsLineArray[5].ToString();
			textBlock_Result06.Text = resultsLineArray[6].ToString();
			textBlock_Result07.Text = resultsLineArray[7].ToString();
			textBlock_Result08.Text = resultsLineArray[8].ToString();
			textBlock_Result09.Text = resultsLineArray[9].ToString();
			textBlock_Result10.Text = resultsLineArray[10].ToString();

			//results debug
			Console.WriteLine("Results: " + resultsLineArray[0] + "|" + resultsLineArray[1] + "|" + resultsLineArray[2] + "|" + resultsLineArray[3] + "|" +
				resultsLineArray[4] + "|" + resultsLineArray[5] + "|" + resultsLineArray[6] + "|" + resultsLineArray[7] + "|" + resultsLineArray[8] + "|" +
				resultsLineArray[9] + "|" + resultsLineArray[10]);
		}

		private void PrintResultsText()
        {
			canvas.Children.Clear();//clear the canvas from the old data
			//if you dont clear the canvas, the old stuff will stay. trippy

			//get the points ready then draw them. 
			points = new Point[11]
			{
			new Point(plotScale*calculationArray[0], plotScale*resultsLineArray[0]),
			new Point(plotScale*calculationArray[1], plotScale*resultsLineArray[1]),
			new Point(plotScale*calculationArray[2], plotScale*resultsLineArray[2]),
			new Point(plotScale*calculationArray[3], plotScale*resultsLineArray[3]),
			new Point(plotScale*calculationArray[4], plotScale*resultsLineArray[4]),
			new Point(plotScale*calculationArray[5], plotScale*resultsLineArray[5]),
			new Point(plotScale*calculationArray[6], plotScale*resultsLineArray[6]),
			new Point(plotScale*calculationArray[7], plotScale*resultsLineArray[7]),
			new Point(plotScale*calculationArray[8], plotScale*resultsLineArray[8]),
			new Point(plotScale*calculationArray[9], plotScale*resultsLineArray[9]),
			new Point(plotScale*calculationArray[10], plotScale*resultsLineArray[10]),
			};
			DrawLine(points);

			//update the result text as a visual
			textBlock_calculationText.Text = listBox1.SelectedItem.ToString() + " with " + 
				IntegerUpDown_detentLocation.Value.ToString() + " Detent";
		}

        private void IntegerUpDown_detentLocation_changed(object sender, DependencyPropertyChangedEventArgs e)
        {
				action_aircraftChanged();
		}

		bool isAircraftListBoxLoaded;

		private void IntegerUpDown_detentLocation_valueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
			//make sure something is selected
			if (isAircraftListBoxLoaded) action_aircraftChanged();
		}

        private void button_close_click(object sender, RoutedEventArgs e)
        {
			Close();//closes the probram when you click the close "button"
		}

        private void titleBar_leftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
			//this moves the window when the titlebar is clicked and held down
			//I made the custom title bar
			if (e.ChangedButton == MouseButton.Left)
				DragMove();
		}
    }
}

