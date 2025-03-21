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
    public partial class GraphEditor : Page
    {
        private Ellipse firstRightClickedElipse = null;
        private Ellipse selectedNode = null;
        private Line createdLine = null;

        public GraphEditor()
        {
            InitializeComponent();
            GraphCanvas.Loaded += (s, e) => InitCanvas();
        }
        private void InitCanvas()
        {
            GraphCanvas.MouseLeftButtonDown += Canva_On_Click_Left;
            GraphCanvas.MouseMove += Canva_On_Mouse_Move;
            createNodeOnWorkspace(new Point(GraphCanvas.ActualWidth/2, GraphCanvas.ActualHeight/2), 50, 50, Brushes.Green);
        }
        private void Canva_On_Click_Left(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(GraphCanvas);
            createNodeOnWorkspace(position,20,20,Brushes.Sienna);
        }
        private void Canva_On_Mouse_Move(object sender, MouseEventArgs e)
        {
            if(createdLine != null)
            {
                Point position = e.GetPosition(GraphCanvas);
                createdLine.X2 = position.X;
                createdLine.Y2 = position.Y;
            }
        }
        private void Elipse_Click_Right(object sender, MouseButtonEventArgs e)
        {
            if(sender is Ellipse clickedEllipse)
            {
                if(createdLine == null)
                {
                    Line line = new Line();
                    line.Stroke = Brushes.Black;
                    line.X1 = Canvas.GetLeft(clickedEllipse) + clickedEllipse.Width / 2;
                    line.Y1 = Canvas.GetTop(clickedEllipse) + clickedEllipse.Height / 2;
                    line.X2 = Canvas.GetLeft(clickedEllipse) + clickedEllipse.Width / 2;
                    line.Y2 = Canvas.GetTop(clickedEllipse) + clickedEllipse.Height / 2;
                    GraphCanvas.Children.Add(line);
                    createdLine = line;
                    firstRightClickedElipse = clickedEllipse;
                }
                else
                {
                    if(firstRightClickedElipse==clickedEllipse)
                    {
                        GraphCanvas.Children.Remove(createdLine);
                        createdLine = null;
                        firstRightClickedElipse = null;
                        return;
                    }
                    createdLine.X2 = Canvas.GetLeft(clickedEllipse) + clickedEllipse.Width / 2;
                    createdLine.Y2 = Canvas.GetTop(clickedEllipse) + clickedEllipse.Height / 2;
                    createdLine = null;
                    actionsAfterCreateLine();
                }

            }
        }
        private void createNodeOnWorkspace(Point position,int width,int height, SolidColorBrush colorBrush)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Width = width;
            ellipse.Height = height;
            ellipse.Fill = colorBrush;

            Canvas.SetLeft(ellipse, position.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, position.Y - ellipse.Height / 2);

            ellipse.MouseRightButtonDown += Elipse_Click_Right;

            Panel.SetZIndex(ellipse, 100);

            GraphCanvas.Children.Add(ellipse);

        }

        private void actionsAfterCreateLine()
        {

        }
    }
}
