﻿using System;
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
using TestApp.ViewModels;

namespace TestApp.Controls
{
    /// <summary>
    /// Interaction logic for ManaFilterControl.xaml
    /// </summary>
    public partial class ManaFilterControl : UserControl
    {
        public ColorFilter Colors
        {
            get { return (ColorFilter)GetValue(ColorsProperty); }
            set { SetValue(ColorsProperty, value); }
        }

        public static readonly DependencyProperty ColorsProperty =
            DependencyProperty.Register("Colors", typeof(ColorFilter), typeof(ManaFilterControl));

        public ManaFilterControl()
        {
            InitializeComponent();
        }
    }
}
