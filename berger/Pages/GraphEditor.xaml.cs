using berger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace berger.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy GraphEditor.xaml
    /// </summary>
    public partial class GraphEditor : UserControl
    {
        private Grid firstRightClickedElipse = null;
        private Grid selectedNode = null;
        private Line createdLine = null;
        private bool isMiddleMouseDown = false;
        private Master master = new Master();
        private Slave slave;
        private bool masterCreated = false;
        private Dictionary<Grid, List<Line>> nodeConnections = [];
        private double zoomFactor = 1.1;
        private double currentScale = 1;
        private List<TextBox> textBoxes;

        public GraphEditor()
        {
            InitializeComponent();
            slave = new Slave("127.0.0.1", master.GetServerPort());
            GraphCanvas.Loaded += (s, e) => InitCanvas();
            master.ClientConnected += OnClientConnected;


            Application.Current.Exit += OnApplicationExit;

            textBoxes = new List<TextBox>
            {
                Bit0, Bit1, Bit2, Bit3,
                Bit4, Bit5, Bit6, Bit7,
                Bit8, Bit9, Bit10, Bit11,
                Bit12, Bit13, Bit14, Bit15
            };
        }
        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            master.StopServer();
        }
        private void InitCanvas()
        {
            //GraphCanvas.MouseLeftButtonDown += Canva_On_Click_Left;
            GraphCanvas.MouseWheel += Canva_MouseWheel;
            GraphCanvas.MouseMove += Canvas_MouseMove;
            GraphCanvas.MouseMove += GraphCanvas_MouseMove;
            GraphCanvas.MouseLeftButtonDown += GraphCanvas_MouseDown;
            GraphCanvas.MouseLeftButtonUp+= GraphCanvas_MouseUp;

            //createNodeOnWorkspace(new Point(GraphCanvas.ActualWidth / 2, GraphCanvas.ActualHeight / 2), 50, 50, Brushes.Green,"1");
            //createNodeOnWorkspace(new Point(GraphCanvas.ActualWidth / 2, GraphCanvas.ActualHeight / 2), 50, 50, Brushes.Green, "2");
        }

        private void OnClientConnected(string clientId)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Random rand = new Random();
                double x = rand.Next(50, (int)GraphCanvas.ActualWidth - 50);
                double y = rand.Next(50, (int)GraphCanvas.ActualHeight - 50);
                if (!masterCreated)
                {
                    CreateNodeOnWorkspace(new Point(GraphCanvas.ActualWidth / 2, GraphCanvas.ActualHeight / 2), 70, 70, Brushes.Green, clientId);
                    masterCreated = true;
                }   
                else
                    CreateNodeOnWorkspace(new Point(x, y), 50, 50, Brushes.Sienna, clientId);
            });
        }
        private void BitInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[01]$");
        }
        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            string bitValue = "";
            foreach (TextBox textBox in textBoxes)
            {
                bitValue += textBox.Text;
            }
            if (bitValue.Length < 16)
            {
                MessageBox.Show("Uzupełnij wszystkie 16 bitów", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //MessageBox.Show($"{bitValue}");

        }
        private void Canva_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scale = (e.Delta > 0) ? zoomFactor : (1.0 / zoomFactor);
            currentScale *= scale;
            foreach (UIElement item in GraphCanvas.Children)
            {
                if (item is Line) continue;
                if (item.RenderTransform is ScaleTransform scaleTransform)
                {
                    scaleTransform.ScaleX *= scale;
                    scaleTransform.ScaleY *= scale;
                }
                else
                {
                    item.RenderTransform = new ScaleTransform(scale, scale);
                }

                if (item is Grid ellipse && nodeConnections.ContainsKey(ellipse))
                {
                    UpdateConnectedLines(ellipse,scale);
                }
            }
        }

        private void UpdateConnectedLines(Grid ellipse, double scale)
        {
            if (!nodeConnections.ContainsKey(ellipse)) return;

            foreach (Line line in nodeConnections[ellipse])
            {
               
                Tuple<Grid, Grid> connection = (Tuple<Grid, Grid>)line.Tag;
                Grid ellipse1 = connection.Item1;
                Grid ellipse2 = connection.Item2;

                Point p1 = GetEllipseCenter(ellipse1);
                Point p2 = GetEllipseCenter(ellipse2);

                line.X1 = p1.X;
                line.Y1 = p1.Y;
                line.X2 = p2.X;
                line.Y2 = p2.Y;
                line.StrokeThickness *= scale;
            }
        }

        private Point GetEllipseCenter(Grid ellipse)
        {
            if (ellipse.RenderTransform is ScaleTransform scaleTransform)
            {
                double centerX = Canvas.GetLeft(ellipse) + (ellipse.Width / 2) * scaleTransform.ScaleX;
                double centerY = Canvas.GetTop(ellipse) + (ellipse.Height / 2) * scaleTransform.ScaleY;
                return new Point(centerX, centerY);
            }
            return new Point(Canvas.GetLeft(ellipse) + ellipse.Width / 2, Canvas.GetTop(ellipse) + ellipse.Height / 2);
        }

        private void Canva_On_Click_Left(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(GraphCanvas);
            CreateNodeOnWorkspace(position, 35, 35, Brushes.Sienna, Guid.NewGuid().ToString());
        }
   

        private void Node_Click_Right(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid clickedEllipse)
            {
                if (clickedEllipse.Name == "Node")  
                {
                    if (createdLine == null)
                    {
                        Line line = new()
                        {
                            Stroke = Brushes.Black,
                            X1 = Canvas.GetLeft(clickedEllipse) + clickedEllipse.Width / 2 * currentScale,
                            Y1 = Canvas.GetTop(clickedEllipse) + clickedEllipse.Height / 2 * currentScale,
                            X2 = Canvas.GetLeft(clickedEllipse) + clickedEllipse.Width / 2 * currentScale,
                            Y2 = Canvas.GetTop(clickedEllipse) + clickedEllipse.Height / 2 * currentScale
                        };
                        GraphCanvas.Children.Add(line);
                        createdLine = line;
                        createdLine.StrokeThickness *= currentScale;
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

                        createdLine.X2 = Canvas.GetLeft(clickedEllipse) + clickedEllipse.Width / 2 * currentScale;
                        createdLine.Y2 = Canvas.GetTop(clickedEllipse) + clickedEllipse.Height / 2 * currentScale;
                        createdLine.StrokeThickness *= currentScale;

                        createdLine.Tag = Tuple.Create(firstRightClickedElipse, clickedEllipse);

                        AddLineToConnections(firstRightClickedElipse, createdLine);
                        AddLineToConnections(clickedEllipse, createdLine);

                        createdLine = null;
                        ActionsAfterCreateLine(firstRightClickedElipse, clickedEllipse);
                    }
                }
            }
            else
            {
                createdLine = null;

            }
        }

        private void CreateNodeOnWorkspace(Point position, int width, int height, SolidColorBrush colorBrush, string clientId = "")
        {
            Grid nodeContainer = new()
            {
                Width = width,
                Height = height,
                Name="Node",
            };

            Ellipse ellipse = new()
            {
                Width = width,
                Height = height,
                Fill = colorBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            Label label = new()
            {
                Content = "65535",
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                IsHitTestVisible = false,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
           };

            nodeContainer.Tag = clientId;

            nodeContainer.MouseRightButtonDown += Node_Click_Right;
            nodeContainer.MouseDown += Node_MouseDown;
            nodeContainer.MouseUp += Node_MouseUp; 

            nodeContainer.Children.Add(ellipse);
            nodeContainer.Children.Add(label);

            Canvas.SetLeft(nodeContainer, position.X - nodeContainer.Width / 2);
            Canvas.SetTop(nodeContainer, position.Y - nodeContainer.Height / 2);

            Panel.SetZIndex(nodeContainer, 100);

            GraphCanvas.Children.Add(nodeContainer);
        }





        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (createdLine != null)
            {
                Point position = e.GetPosition(GraphCanvas);
                createdLine.X2 = position.X;
                createdLine.Y2 = position.Y;
            }
            if (selectedNode != null)
            {
                Point position = e.GetPosition(GraphCanvas);
                if (selectedNode.RenderTransform is ScaleTransform scaleTransform)
                {
                    Canvas.SetLeft(selectedNode, Math.Max(0, Math.Min(GraphCanvas.ActualWidth - selectedNode.Width * scaleTransform.ScaleX, position.X - selectedNode.Width / 2 * scaleTransform.ScaleX)));
                    Canvas.SetTop(selectedNode, Math.Max(0, Math.Min(GraphCanvas.ActualHeight - selectedNode.Height * scaleTransform.ScaleX, position.Y - selectedNode.Height / 2 * scaleTransform.ScaleY)));
                  
                }
                else
                {
                    Canvas.SetLeft(selectedNode, Math.Max(0, Math.Min(GraphCanvas.ActualWidth - selectedNode.Width, position.X - selectedNode.Width / 2)));
                    Canvas.SetTop(selectedNode, Math.Max(0, Math.Min(GraphCanvas.ActualHeight - selectedNode.Height, position.Y - selectedNode.Height / 2)));

                }
                if (nodeConnections.TryGetValue(selectedNode, out List<Line> lines))
                {
                    foreach (var line in lines)
                    {
                        if (line.Tag is Tuple<Grid, Grid> endpoints)
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



        private void AddLineToConnections(Grid node, Line line)
        {
            if (!nodeConnections.TryGetValue(node, out List<Line>? value))
            {
                value = [];
                nodeConnections[node] = value;
            }

            value.Add(line);
        }

        private void Node_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Released)
            {
                selectedNode = null;
                isMiddleMouseDown = false;
                // canva.ReleaseMouseCapture(); // Zakończ przechwytywanie myszy
            }
        }
        private void Node_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed && sender is Grid selectedEllipse)
            {
                selectedNode = selectedEllipse;
                isMiddleMouseDown = true;
                // startPanPoint = e.GetPosition(canva);
                // canva.CaptureMouse(); // Upewnij się, że otrzymujesz zdarzenia myszy nawet poza Canvas
            }
           
        }
        private bool isPanning = false;
        private Point lastMousePosition;


        private void GraphCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isPanning = true;
                lastMousePosition = e.GetPosition(GraphCanvas);
                GraphCanvas.CaptureMouse();
            }
        }

        private void GraphCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                bool canPanLeft = true;
                bool canPanUp = true;
                bool canPanRight = true;
                bool canPanDown = true;

                Point currentMousePosition = e.GetPosition(GraphCanvas);
                double offsetX = currentMousePosition.X - lastMousePosition.X;
                double offsetY = currentMousePosition.Y - lastMousePosition.Y;

                foreach (UIElement element in GraphCanvas.Children)
                {
                    if (element is Line) continue;

                    double left = Canvas.GetLeft(element);
                    double top = Canvas.GetTop(element);

                    if (double.IsNaN(left)) left = 0;
                    if (double.IsNaN(top)) top = 0;

                    double elementWidth = 0;
                    double elementHeight = 0;

                    if (element is FrameworkElement fe)
                    {
                        elementWidth = fe.ActualWidth;
                        elementHeight = fe.ActualHeight;
                    }

                    if (left <= 0) canPanLeft = false;
                    if (top <= 0) canPanUp = false;
                    if (left + elementWidth * currentScale >= GraphCanvas.ActualWidth) canPanRight = false;
                    if (top + elementHeight * currentScale >= GraphCanvas.ActualHeight) canPanDown = false;
                }

                if (!canPanLeft && offsetX < 0) offsetX = 0;
                if (!canPanRight && offsetX > 0) offsetX = 0;
                if (!canPanUp && offsetY < 0) offsetY = 0;
                if (!canPanDown && offsetY > 0) offsetY = 0;

                foreach (UIElement element in GraphCanvas.Children)
                {
                    if (element is Line line)
                    {
                        line.X1 += offsetX;
                        line.Y1 += offsetY;
                        line.X2 += offsetX;
                        line.Y2 += offsetY;
                    }
                    else
                    {
                        double left = Canvas.GetLeft(element);
                        double top = Canvas.GetTop(element);

                        if (double.IsNaN(left)) left = 0;
                        if (double.IsNaN(top)) top = 0;

                        Canvas.SetLeft(element, left + offsetX);
                        Canvas.SetTop(element, top + offsetY);
                    }
                }

                lastMousePosition = currentMousePosition;
            }
        }


   



        private void GraphCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isPanning = false;
            GraphCanvas.ReleaseMouseCapture();
        }



        private void ActionsAfterCreateLine(Grid firstRightClickedElipse, Grid clickedEllipse)
        {
            master.SendConnectionDetailsMessage(firstRightClickedElipse.Tag.ToString(), clickedEllipse.Tag.ToString()); 
        }
    }
}
