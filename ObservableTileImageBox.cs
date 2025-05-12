using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MahjongApp
{
    public class ObservableTilePictureBox : PictureBox, ITilePictureBoxSubject
    {
        private readonly List<ITilePictureBoxObserver> observers = new List<ITilePictureBoxObserver>();

        public void RegisterObserver(ITilePictureBoxObserver observer)
        {
            observers.Add(observer);;
        }

        public void RemoveObserver(ITilePictureBoxObserver observer)
        {
            observers.Remove(observer);
        }

        public void NotifyObservers()
        {
            foreach (var observer in observers)
            {
                observer.OnPictureBoxClicked();
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            NotifyObservers();
        }
    }
}
