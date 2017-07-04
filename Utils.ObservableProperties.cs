/**
 * MIT License
 * 
 * Copyright (c) 2017 "Nabil Redmann (BananaAcid) <repo@bananaacid.de>"
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Utils
{
    /// <summary>
    /// have single properties to be observable (bindable) in wpf.
    /// you have to bind the underlying value variable.
    /// 
    /// this is the opposite to the MSDN, stating, you should have a specific class, that is observable and should hold all required properties to be reflected by the WPF-XAML-UI.
    /// </summary>
    /// <see cref="http://stackoverflow.com/a/43835207/1644202"/>
    public static class ObservableProperties
    {
        /// <summary>
        /// a single observable type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <example>
        /// using static Utils.ObservableProperties;
        /// 
        /// Class MainWindow ...
        ///     public static Prop<bool> actionIsLoading { get; set; } = new Prop<bool>();                   // without initial value
        ///     public static Prop<bool> actionIsLoading { get; set; } = new Prop<bool> { Value = false };   // with initial value
        ///     
        ///     // initialize with user settings, need to be saved back on app exit
        ///     public static Prop<bool> optionStartMinimized { get; set; } = new Prop<bool> { Value = Properties.Settings.Default.optionStartMinimized };
        ///     // save it back to user settings:
        ///     //   Properties.Settings.Default.optionStartMinimized = optionStartMinimized.Value;
        ///     //   Properties.Settings.Default.save();
        /// XAML
        ///     Value="{Binding actionIsLoading.Value}"
        /// </example>
        public class Prop<T> : ObservablePropertyBase
        {
            internal T _value;
            public T Value
            {
                get { return _value; }
                set { _value = value; NotifyPropertyChanged(); }
            }

            public static bool operator ==(Prop<T> self, T other)
            {
                return self._value.Equals(other);
            }

            public static bool operator !=(Prop<T> self, T other)
            {
                return !self._value.Equals(other);
            }

            public void NotifyPropertyChanged()
            {
                NotifyPropertyChanged("Value");
            }
        }

        /// <summary>
        /// a double type
        /// </summary>
        /// <typeparam name="T">internal Value number 1's type</typeparam>
        /// <typeparam name="U">internal Value2 number 2's type </typeparam>
        /// <example>
        /// using static Utils.ObservableProperties;
        /// 
        /// Class MainWindow ...
        ///     public static PropProp<Visibility, int> isUpdateProgress { get; set; } = new PropProp<Visibility, int> { Value = Visibility.Hidden; Value2 = 0; };
        /// XAML
        ///     ... Value="{Binding isUpdateProgress.Value}"  ...
        ///     ... Value="{Binding isUpdateProgress.Value2}"  ...
        /// </example>
        public class PropProp<T, U> : Prop<T>
        {
            internal U _value2;
            public U Value2
            {
                get { return _value2; }
                set { _value2 = value; NotifyPropertyChanged(); }
            }

            public new void NotifyPropertyChanged()
            {
                NotifyPropertyChanged("Value");
                NotifyPropertyChanged("Value2");
            }
        }



        /// <summary>
        /// internal.
        /// </summary>
        public class ObservablePropertyBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            internal void NotifyPropertyChanged(String propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }

    /// <summary>
    /// extending observable collections to support ranges
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <example>
    /// Class MainWindow ...
    ///     public static ObservableCollection<string> optionPathList { get; set; }
    /// XAML
    /// ... ItemsSource="{Binding optionPathList}" ...
    /// </example>
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                this.Items.Add(item);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
        }

        public void NotifyCollectionChanged()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
