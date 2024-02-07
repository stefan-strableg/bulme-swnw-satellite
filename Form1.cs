
using System;
using System.Collections;
using System.Collections.Generic;
// using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MV;

namespace satellite
{
    public partial class Form1 : Form
    {
        private double m_Speed, m_Direction;
        List<Satellite> m_Satellites = new List<Satellite>();

        public Form1()
        {
            InitializeComponent();
            m_pnl.Init();
            Satellite.SetDefaultColors(this.BackColor, Color.Red);
            timer1.Interval = 20; timer1.Start();
            m_ZoomTrk.Value = 100;
            m_Ed1.Text = "500  0";
        }

        void SpeedAndDirFromText()
        {
            string txt, str1, str2;
            int p1;

            txt = m_Ed1.Text; txt += "  ";
            for (p1 = 0; txt[p1] != ' '; p1++)
            {
                if (p1 >= txt.Length)
                    return;
            }
            str1 = txt.Substring(0, p1);
            str2 = txt.Substring(p1);

            m_Speed = double.Parse(str1); m_Direction = double.Parse(str2);
        }

        private void OnPanelPaint(object sender, PaintEventArgs e)
        {
            foreach (Satellite satellite in m_Satellites)
            {
                satellite.PaintVisible(e.Graphics);
            }
        }

        void CalcNextPositions()
        {
            //Vect2D rVect, acc;
            //double rDist;
            //for (int i = 0; i < Par.ITER_PER_TICK; i++)
            //{
            //    // Vector von der Erde zum Satteliten
            //    rVect = m_Earth.VectBetweenObjects(m_Sat);

            //    rDist = rVect.GetR(); // Abstand  Erde Sattelit

            //    // rVect auf 1 normiert ( Länge == 1 )
            //    rVect = rVect.ScalarMult(1 / rDist);

            //    // Gravitationsvektor der auf den Sat wirkt m*g/r^2
            //    acc = rVect.ScalarMult(Par.EARTH_ACCEL / (rDist * rDist));

            //    // Vn+1 = Vn - acc*DT
            //    m_Sat.m_V.SubFrom(acc);

            //    // Xn+1 = Xn + Vn*DT
            //    m_Sat.IntegratePosition();
            //}
            //m_Sat.AddTracePoint();

            List<Satellite> newSatellites = new List<Satellite>(m_Satellites);

            for (int a = 0; a < m_Satellites.Count; a++)
            {
                Satellite satA = m_Satellites[a];
                for (int b = 0; b < m_Satellites.Count; b++)
                {
                    Satellite satB = newSatellites[b];

                    if (satA == satB)
                        continue;

                    Vect2D rV = satA.VectBetweenObjects(satB);

                    double r = rV.GetR(); // Abstand  Erde Sattelit

                    if (r < satA.Radius + satB.Radius)
                        continue;


                    rV = rV.ScalarMult(1 / r);

                    if (r < 1)
                        r = 1;


                    Vect2D acc = rV.ScalarMult(-Par.G * 1E13 / (r * r * 0.2));

                    satB.m_V += acc;
                }
            }


            /*for (int a = 0; a < m_Satellites.Count; a++)
            {
                Satellite satA = m_Satellites[a];
                for (int b = a; b < m_Satellites.Count; b++)
                {
                    Satellite satB = newSatellites[b];

                    double r = satA.VectBetweenObjects(satB).GetR();

                    if (r < satA.Radius + satB.Radius)
                    {
                        satA.m_V = satA.m_V.GetOppositeDirection();
                        satB.m_V = satB.m_V.GetOppositeDirection();
                    }
                }
            }*/


            foreach (Satellite satellite in newSatellites)
            {
                satellite.IntegratePosition();
            }


            m_Satellites = newSatellites;
        }

        void timer1_Tick(object sender, EventArgs e)
        {
            if (m_RunChk.Checked)
            {
                CalcNextPositions();
                m_pnl.Invalidate();
            }
        }

        void OnStepButt(object sender, EventArgs e)
        {
            m_RunChk.Checked = false;
            CalcNextPositions();
            m_pnl.Invalidate();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            m_Satellites.Clear();
        }

        private void OnPanelMouseDown(object sender, MouseEventArgs e)
        {
            Point pos = m_pnl.Device2World(e.Location);
            SpeedAndDirFromText();
            Satellite newSatellite = new SatelliteWithTrace(pos, Color.Blue, m_Speed / Par.ITER_PER_TICK, m_Direction);
            newSatellite.Radius = 10;
            m_Satellites.Add(newSatellite);
            m_pnl.Invalidate();
        }

        private void OnZoomChanged(object sender, EventArgs e)
        {
            float val = ((float)m_ZoomTrk.Value / 100.0F);
            m_lbl2.Text = val.ToString();
            m_pnl.ScaleFactor = val;
            m_pnl.Invalidate();
        }
    }
}