using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Affins
{
    public partial class Form1 : Form
    {
        Bitmap bmp;
        Pen pen = new Pen(Color.Blue, 4);
        Graphics g;
        public Form1()
        {
            InitializeComponent();
            bmp = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            pictureBox1.Image = bmp;
            pen.EndCap = LineCap.ArrowAnchor;
            g = Graphics.FromImage(bmp);
        }


        List<My_point> points = new List<My_point>(); // список точек
        List<Edge> edges = new List<Edge>(); // список рёбер
        Polygon pol = new Polygon();
        List<Polygon> triangles = new List<Polygon>();
        SolidBrush brush = new SolidBrush(Color.Black);
        bool newPol = true;

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (CheckBox1.Checked)
                {
                    if (newPol)
                    {
                        pol = new Polygon();
                        newPol = false;
                        My_point p = new My_point(e.X, e.Y);
                        pol.points.Add(p);
                    }
                    else
                    {
                        My_point p = new My_point(e.X, e.Y);
                        pol.points.Add(p);
                        if (pol.points.Count > 2)
                        {
                            pol.edges.Remove(pol.edges.tail);
                        }
                        Edge edg = new Edge(pol.points.tail.Previous.Data, p);
                        Edge edg2 = new Edge(pol.points.tail.Data, pol.points.head.Data);
                        pol.edges.Add(edg);
                        pol.edges.Add(edg2);
                    }
                    redrawImage();
                }
                else
                {
                    My_point p = null;
                    foreach (My_point pp in points)
                        if (Math.Abs(pp.X - e.X) <= 3 && Math.Abs(pp.Y - e.Y) <= 3)
                            p = pp;
                    if (p == null)
                    {
                        My_point new_p = new My_point(e.X, e.Y);
                        points.Add(new_p);
                        redrawImage();
                    }
                }
            }
        }

        My_point edgeStart;
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            edgeStart = null;
            foreach (My_point p in points)
                if (Math.Abs(p.X - e.X) <= 3 && Math.Abs(p.Y - e.Y) <= 3)
                    edgeStart = p;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (edgeStart != null)
            {
                My_point edgeEnd = null;
                foreach (My_point p in points)
                    if (Math.Abs(p.X - e.X) <= 3 && Math.Abs(p.Y - e.Y) <= 3)
                        edgeEnd = p;
                if (edgeEnd != null)
                {
                    Edge ed = new Edge(edgeStart, edgeEnd);
                    edges.Add(ed);
                    ed.Draw(g, pen);
                    redrawImage();
                }
            }
        }

        private void redrawImage()
        {
            g.Clear(Color.White);
            foreach (My_point p in points)
                p.Draw(g);
            foreach (Edge e in edges)
                e.Draw(g, pen);
            if (pol != null)
             pol.Draw(g, pen);
            pictureBox1.Image = bmp;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            points.Clear();
            edges.Clear();
            pol = null;
            newPol = true;
            pictureBox1.Image = bmp;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if ((edges.Count != 1) || (points.Count != 3))
            {
                MessageBox.Show("Нарисуйте только ребро и точку!", "Ошибка!");
                return;
            }

            int z = posToEdge(edges[0], points[2]);
            if (z == -1)
                MessageBox.Show("Точка находится справа от ребра", "Результат");
            else
            if (z == 1)
                MessageBox.Show("Точка находится слева от ребра", "Результат");
            else
                MessageBox.Show("Точка находится на ребре", "Результат");
        }
        private int posToEdge(Edge ed, My_point pt)
        {
            double z = (ed.P2.Y - ed.P1.Y) * pt.X + (ed.P1.X - ed.P2.X) * pt.Y + (ed.P1.X * (ed.P1.Y - ed.P2.Y) + ed.P1.Y * (ed.P2.X - ed.P1.X));
            if (z < 0)
                return -1;
            else
                if (z > 0)
                return 1;
            else
                return 0;
        }

       

        private void button4_Click(object sender, EventArgs e)
        {
            if ((pol == null) || (points.Count < 1))
            {
                MessageBox.Show("Нарисуйте только многоугольник и точку!", "Ошибка!");
                return;
            }
            My_point pt = points[points.Count - 1];
            bool fromRight = true;
            foreach (Edge ed in pol.edges)
                if (posToEdge(ed, pt) != -1)
                {
                    fromRight = false;
                    break;
                }
            if (fromRight)
                MessageBox.Show("Точка находится внутри многоугольника", "Положение точки");
            else
                MessageBox.Show("Точка находится вне многоугольника", "Положение точки");
        }


        private void button3_Click_1(object sender, EventArgs e)
        {
            triangulate(pol);
            foreach(Polygon triangle in triangles)
            {
                triangle.Draw(g, pen);
            }
        }

        private void triangulate(Polygon plg)
        {
            if (plg.points.Count() == 3)
            {
                triangles.Add(plg);
            }
            else
            {
                int pos = 1;
                DoublyNode<My_point> cur = plg.points.head;
                while (pos != -1)
                {
                    cur = cur.Next;
                    Edge edg = new Edge(cur.Previous.Data, cur.Data);
                    pos = posToEdge(edg, cur.Next.Data);
                    cur = cur.Next;
                }
                Polygon tPol = new Polygon();
                tPol.points.Add(cur.Previous);
                tPol.points.Add(cur);
                tPol.points.Add(cur.Next);
                tPol.edges.Add(new Edge(cur.Previous.Data, cur.Data));
                tPol.edges.Add(new Edge(cur.Data, cur.Next.Data));
                tPol.edges.Add(new Edge(cur.Next.Data, cur.Previous.Data));
                DoublyNode<My_point> temp = plg.points.head;
                bool inside = true;
                while (temp != null)
                {
                    foreach (Edge ed in tPol.edges)
                        if (posToEdge(ed, temp.Data) != -1)
                        {
                            inside = false;
                            break;
                        }
                    if (!inside)
                        break;
                    else
                        temp = temp.Next;
                }
                if (inside)
                {

                }
                else
                {
                    triangles.Add(tPol);
                }
            }
        }

    }

    public class My_point
    {
        public int X, Y;
        public My_point(int x, int y) { X = x; Y = y; }
        public static bool operator ==(My_point p1, My_point p2)
        {
            if (System.Object.ReferenceEquals(p1, p2))
                return true;
            if (((object)p1 == null) || ((object)p2 == null))
                return false;
            return p1.X == p2.X && p1.Y == p2.Y;
        }
        public static bool operator !=(My_point p1, My_point p2)
        {
            return !(p1 == p2);
        }
        public void Draw(Graphics g)
        {
            g.FillEllipse(new SolidBrush(Color.Black), X - 4, Y - 4, 8, 8);
        }
    }

    public class Edge
    {
        public My_point P1, P2;
        public Edge(My_point p1, My_point p2) { P1 = p1; P2 = p2; }
        public bool contains(My_point p) { return p == P1 || p == P2; }
        public My_point start() { return P1; }
        public My_point end() { return P2; }
        public void Draw(Graphics g, Pen pen)
        {
            g.DrawLine(pen, P1.X, P1.Y, P2.X, P2.Y);
        }
    }



    class Polygon
    {
        public DoublyLinkedList<My_point> points = new DoublyLinkedList<My_point>();
        public DoublyLinkedList<Edge> edges = new DoublyLinkedList<Edge>();
      


        public Polygon() { }

        public Polygon(DoublyLinkedList<My_point> points)
        {
            this.points = points;
        }

        public void Draw(Graphics g, Pen pen)
        {
            if (points.Count == 0)
                return;
            if (1 == points.Count)
                points.head.Data.Draw(g);
            else
            {
                
                foreach (Edge edg in edges)
                    g.DrawLine(pen, edg.P1.X, edg.P1.Y, edg.P2.X, edg.P2.Y);
            }
        }

        public My_point findConvexPoint()
        {
            return points.head.Data;
        }
    }
    public class DoublyNode<T>
    {
        public DoublyNode(T data)
        {
            Data = data;
        }
        public T Data { get; set; }
        public DoublyNode<T> Previous { get; set; }
        public DoublyNode<T> Next { get; set; }
    }

    public class DoublyLinkedList<T> : IEnumerable<T>  // двусвязный список
    {
        public DoublyNode<T> head; // головной/первый элемент
        public DoublyNode<T> tail; // последний/хвостовой элемент
        int count;  // количество элементов в списке

        // добавление элемента
        public void Add(T data)
        {
            DoublyNode<T> node = new DoublyNode<T>(data);

            if (head == null)
                head = node;
            else
            {
                tail.Next = node;
                node.Previous = tail;
            }
            tail = node;
            count++;
        }
        public void Add(DoublyNode<T> node)
        {
            if (head == null)
                head = node;
            else
            {
                tail.Next = node;
                node.Previous = tail;
            }
            tail = node;
            count++;
        }
        public void AddFirst(T data)
        {
            DoublyNode<T> node = new DoublyNode<T>(data);
            DoublyNode<T> temp = head;
            node.Next = temp;
            head = node;
            if (count == 0)
                tail = head;
            else
                temp.Previous = node;
            count++;
        }
        public DoublyNode<T> AddBefore(T data, DoublyNode<T> current)
        {
            DoublyNode<T> toAdd = new DoublyNode<T>(data);
            if (current.Previous != null)
            {
                current.Previous.Next = toAdd;
                toAdd.Previous = current.Previous;
            }
            else
            {
                head = toAdd;
            }
            current.Previous = toAdd;
            toAdd.Next = current;
            return toAdd;
        }
        // удаление
        public bool Remove(DoublyNode<T> current)
        {
            if (current != null)
            {
                // если узел не последний
                if (current.Next != null)
                {
                    current.Next.Previous = current.Previous;
                }
                else
                {
                    // если последний, переустанавливаем tail
                    tail = current.Previous;
                }

                // если узел не первый
                if (current.Previous != null)
                {
                    current.Previous.Next = current.Next;
                }
                else
                {
                    // если первый, переустанавливаем head
                    head = current.Next;
                }
                count--;
                return true;
            }
            return false;
        }

        public int Count { get { return count; } }
        public bool IsEmpty { get { return count == 0; } }

        public void Clear()
        {
            head = null;
            tail = null;
            count = 0;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this).GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            DoublyNode<T> current = head;
            while (current != null)
            {
                yield return current.Data;
                current = current.Next;
            }
        }

        public IEnumerable<T> BackEnumerator()
        {
            DoublyNode<T> current = tail;
            while (current != null)
            {
                yield return current.Data;
                current = current.Previous;
            }
        }
    }
}
