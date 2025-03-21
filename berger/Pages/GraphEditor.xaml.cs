using berger.Threads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace berger.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy GraphEditor.xaml
    /// </summary>
    public partial class GraphEditor : UserControl
    {
        private Ellipse firstRightClickedElipse = null;
        private Ellipse selectedNode = null;
        private Line createdLine = null;
        private bool isMiddleMouseDown = false;
        public static ListenerThread listenerThread = new ListenerThread(true);


        public GraphEditor()
        {
            InitializeComponent();
            GraphCanvas.Loaded += (s, e) => InitCanvas();
            listenerThread.start();
            Application.Current.Exit += OnApplicationExit;
        }
        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            listenerThread.stop();
        }
        private void InitCanvas()
        {
            GraphCanvas.MouseLeftButtonDown += Canva_On_Click_Left;
            GraphCanvas.MouseMove += Canva_On_Mouse_Move;
            GraphCanvas.MouseMove += Canvas_MouseMove;
            createNodeOnWorkspace(new Point(GraphCanvas.ActualWidth / 2, GraphCanvas.ActualHeight / 2), 50, 50, Brushes.Green);
        }



        private void Canva_On_Click_Left(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(GraphCanvas);
            createNodeOnWorkspace(position, 20, 20, Brushes.Sienna);
        }
        private void Canva_On_Mouse_Move(object sender, MouseEventArgs e)
        {
            if (createdLine != null)
            {
                Point position = e.GetPosition(GraphCanvas);
                createdLine.X2 = position.X;
                createdLine.Y2 = position.Y;
            }
        }

        private void Elipse_Click_Right(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse clickedEllipse)
            {
                if (createdLine == null)
                {
                    Line line = new()
                    {
                        Stroke = Brushes.Black,
                        X1 = Canvas.GetLeft(clickedEllipse) + clickedEllipse.Width / 2,
                        Y1 = Canvas.GetTop(clickedEllipse) + clickedEllipse.Height / 2,
                        X2 = Canvas.GetLeft(clickedEllipse) + clickedEllipse.Width / 2,
                        Y2 = Canvas.GetTop(clickedEllipse) + clickedEllipse.Height / 2
                    };

                    GraphCanvas.Children.Add(line);
                    createdLine = line;
                    firstRightClickedElipse = clickedEllipse;
                }
                else
                {
                    if (firstRightClickedElipse == clickedEllipse)
                    {
                        GraphCanvas.Children.Remove(createdLine);
                        createdLine = null;
                        firstRightClickedElipse = null;
                        return;
                    }

                    createdLine.X2 = Canvas.GetLeft(clickedEllipse) + clickedEllipse.Width / 2;
                    createdLine.Y2 = Canvas.GetTop(clickedEllipse) + clickedEllipse.Height / 2;

                    createdLine.Tag = Tuple.Create(firstRightClickedElipse, clickedEllipse);
                    AddLineToConnections(firstRightClickedElipse, createdLine);
                    AddLineToConnections(clickedEllipse, createdLine);

                    createdLine = null;
                    actionsAfterCreateLine();
                }
            }
        }
        private void createNodeOnWorkspace(Point position, int width, int height, SolidColorBrush colorBrush)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Width = width;
            ellipse.Height = height;
            ellipse.Fill = colorBrush;

            Canvas.SetLeft(ellipse, position.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, position.Y - ellipse.Height / 2);

            ellipse.MouseRightButtonDown += Elipse_Click_Right;
            ellipse.MouseDown += Canvas_MouseDown;
            ellipse.MouseUp += Canvas_MouseUp;

            Panel.SetZIndex(ellipse, 100);

            GraphCanvas.Children.Add(ellipse);

        }


        private Dictionary<Ellipse, List<Line>> nodeConnections = [];

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedNode != null)
            {
                Point position = e.GetPosition(GraphCanvas);
                Canvas.SetLeft(selectedNode, position.X - selectedNode.Width / 2);
                Canvas.SetTop(selectedNode, position.Y - selectedNode.Height / 2);

                if (nodeConnections.TryGetValue(selectedNode, out List<Line> lines))
                {
                    foreach (var line in lines)
                    {
                        if (line.Tag is Tuple<Ellipse, Ellipse> endpoints)
                        {
                            if (endpoints.Item1 == selectedNode)
                            {
                                line.X1 = position.X;
                                line.Y1 = position.Y;
                            }
                            else if (endpoints.Item2 == selectedNode)
                            {
                                line.X2 = position.X;
                                line.Y2 = position.Y;
                            }
                        }
                    }
                }
            }
        }

 

        private void AddLineToConnections(Ellipse node, Line line)
        {
            if (!nodeConnections.TryGetValue(node, out List<Line>? value))
            {
                value = [];
                nodeConnections[node] = value;
            }

            value.Add(line);
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Released)
            {
                selectedNode = null;
                isMiddleMouseDown = false;
                // canva.ReleaseMouseCapture(); // Zakończ przechwytywanie myszy
            }
        }
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed && sender is Ellipse selectedEllipse)
            {
                selectedNode = selectedEllipse;
                isMiddleMouseDown = true;
                // startPanPoint = e.GetPosition(canva);
                // canva.CaptureMouse(); // Upewnij się, że otrzymujesz zdarzenia myszy nawet poza Canvas
            }
        }


        private void actionsAfterCreateLine()
        {

        }
    }
}
