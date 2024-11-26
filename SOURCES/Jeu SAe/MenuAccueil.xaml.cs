using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Jeu_SAe
{
    /// <summary>
    /// Logique d'interaction pour MenuAccueil.xaml
    /// </summary>
    public partial class MenuAccueil : Window
    {
        public DispatcherTimer minuterieDuJeu = new DispatcherTimer();

        // vitesse effet scrole du fond et du sol
        public int vitesseFond = 3;
        public int vitesseSol = 5;

        // variables utlisées pour le changement de cartes
        private string numCarte = "1";
        private bool changementCarte = false;

        // variables image pour le fond et le sol
        private ImageBrush imgFond = new ImageBrush();
        private ImageBrush imgSol = new ImageBrush();
        private ImageBrush imgCarte1 = new ImageBrush();
        private ImageBrush imgCarte2 = new ImageBrush();
        private ImageBrush imgCarte3 = new ImageBrush();

        // varirable pour le choix de la diffculté
        public string difficulteChoisie = "Facile";

        // variable pour le son en arrière plan
        public SoundPlayer backgroundSound = new SoundPlayer();

        // variables pour le changement de touches
        private bool changementTouche = false;
        private bool toucheChangee = false;
        private string toucheAffichee = "Espace";
        public Key touche = Key.Space;

        public MenuAccueil()
        {
            InitializeComponent();

            minuterieDuJeu.Tick += MoteurdeJeu;
            minuterieDuJeu.Interval = TimeSpan.FromMilliseconds(20);
            minuterieDuJeu.Start();

            imgFond.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "images/fond1.jpg"));
            fond1.Fill = imgFond;
            fond2.Fill = imgFond;
            fond3.Fill = imgFond;

            imgSol.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "images/sol1.jpg"));
            sol1.Fill = imgSol;
            sol2.Fill = imgSol;
            sol3.Fill = imgSol;

            imgCarte1.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "images/carte1.jpg"));
            butCarte1.Background = imgCarte1;

            imgCarte2.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "images/carte2.jpg"));
            butCarte2.Background = imgCarte2;

            imgCarte3.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "images/carte3.jpg"));
            butCarte3.Background = imgCarte3;

            canvaMenu.Visibility = Visibility.Visible;
            canvaJouer.Visibility = Visibility.Hidden;
            canvaParametres.Visibility = Visibility.Hidden;
        }

        private void MoteurdeJeu(object sender, EventArgs e)
        {
            MouvementFond(fond1, fond2, fond3);
            MouvementSol(sol1, sol2, sol3);

            if (changementCarte)
            {
                ChangerCarte();
                ChangerSon();
            }

            if (changementTouche)
                ChangerTouche();
        }

        private void CanvasKeyIsDown(object sender, KeyEventArgs e)
        {
            // Cette méthode est utlisé afin d'attribuer la nouvelle touche
            if (changementTouche)
            {
                if (e.Key == Key.Space)
                    toucheAffichee = "Espace";
                else if (e.Key != Key.Escape)
                {
                    toucheAffichee = e.Key.ToString();
                    touche = e.Key;
#if DEBUG 
                    Console.WriteLine("La nouvelle touche est : " + touche.ToString());
#endif
                }
                toucheChangee = true;
            }
        }

        public void MouvementFond(Rectangle fond1, Rectangle fond2, Rectangle fond3)
        {
            // Cette méthode permet de mettre le fond en mouvement
            Canvas.SetLeft(fond1, Canvas.GetLeft(fond1) - vitesseFond);
            Canvas.SetLeft(fond2, Canvas.GetLeft(fond2) - vitesseFond);
            Canvas.SetLeft(fond3, Canvas.GetLeft(fond3) - vitesseFond);

            // Si la position gauche du fond est plus petite que -(la longueur du fond) (quand le fond sort entièrement de l'écran)
            if (Canvas.GetLeft(fond1) < - fond1.ActualWidth)
            {
                Canvas.SetLeft(fond1, Canvas.GetLeft(fond3) + fond3.ActualWidth);
            }

            if (Canvas.GetLeft(fond2) < - fond2.ActualWidth)
            {
                Canvas.SetLeft(fond2, Canvas.GetLeft(fond1) + fond1.ActualWidth);
            }

            if (Canvas.GetLeft(fond3) < - fond3.ActualWidth)
            {
                Canvas.SetLeft(fond3, Canvas.GetLeft(fond2) + fond2.ActualWidth);
            }
        }

        public void MouvementSol(Rectangle sol1, Rectangle sol2, Rectangle sol3)
        {
            // Cette méthode permet de mettre le sol en mouvement
            Canvas.SetLeft(sol1, Canvas.GetLeft(sol1) - vitesseSol);
            Canvas.SetLeft(sol2, Canvas.GetLeft(sol2) - vitesseSol);
            Canvas.SetLeft(sol3, Canvas.GetLeft(sol3) - vitesseSol);

            if (Canvas.GetLeft(sol1) < - sol1.ActualWidth)
            {
                Canvas.SetLeft(sol1, Canvas.GetLeft(sol3) + sol3.ActualWidth);
            } 

            if (Canvas.GetLeft(sol2) < - sol2.ActualWidth)
            {
                Canvas.SetLeft(sol2, Canvas.GetLeft(sol1) + sol1.ActualWidth);
            }
            
            if (Canvas.GetLeft(sol3) < - sol3.ActualWidth)
            {
                Canvas.SetLeft(sol3, Canvas.GetLeft(sol2) + sol2.ActualWidth);
            }
        }

        private void ValidationBouton(Button boutonChoisi, Button boutonNonChoisi, Button boutonNonChoisi2)
        {
            // Cette méthode permet de selectionner visuellement le bouton séléctionné
            boutonChoisi.BorderBrush = Brushes.Blue;
            boutonNonChoisi.BorderBrush = Brushes.Beige;
            boutonNonChoisi2.BorderBrush = Brushes.Beige;   
        }

        private void ChangerTouche()
        {
            // Cette méthode permet de changer de touches
            // En attente qu'une touche soit choisie
            if (!toucheChangee)
            {
                butTouche.Content = "Appuyer sur une touche...";
                butTouche.FontSize = 10;
                butTouche.Foreground = new SolidColorBrush(Color.FromRgb(107, 107, 107)); //#FF6B6B6B, Gris Foncé
                lblEchap.Visibility = Visibility.Visible;
                toucheChangee = false;
            }
            else
            {
                butTouche.Content = toucheAffichee;
                butTouche.FontSize = 36;
                butTouche.Foreground = new SolidColorBrush(Colors.Black);
                lblEchap.Visibility = Visibility.Hidden;
                changementTouche = false;
            }
            // Si la souris detecte un clique alors la touche revient à la normale
            if (Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed)
                toucheChangee = true;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas.SetLeft(canvaMenu, myCanva.ActualWidth / 2 - canvaMenu.Width / 2);
            Canvas.SetLeft(canvaJouer, myCanva.ActualWidth / 2 - canvaJouer.Width / 2);
            Canvas.SetLeft(canvaParametres, myCanva.ActualWidth / 2 - canvaParametres.Width / 2);
        }

        private void ChangerCarte()
        {
            imgFond.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "images/fond" + numCarte + ".jpg"));
            fond1.Fill = imgFond;
            fond2.Fill = imgFond;

            imgSol.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "images/sol" + numCarte + ".jpg"));
            sol1.Fill = imgSol;
            sol2.Fill = imgSol;

            changementCarte = false;
        }

        private void ChangerSon()
        {
            backgroundSound = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + "sound\\backgroundSound" + numCarte + ".wav");
        }

        // Boutons de difficultés
        private void butDifficulteFacile_Click(object sender, RoutedEventArgs e)
        {
            difficulteChoisie = "Facile";
            ValidationBouton(butDifficulteFacile, butDifficulteMoyenne, butDifficulteDifficile);
        }

        private void butDifficulteMoyenne_Click(object sender, RoutedEventArgs e)
        {
            difficulteChoisie = "Moyen";
            ValidationBouton(butDifficulteMoyenne, butDifficulteDifficile, butDifficulteFacile);
        }

        private void butDifficulteDifficile_Click(object sender, RoutedEventArgs e)
        {
            difficulteChoisie = "Difficile";
            ValidationBouton(butDifficulteDifficile, butDifficulteMoyenne, butDifficulteFacile);
        }

        // Premier bouton jouer dans le menu
        private void ButJouer_Click(object sender, RoutedEventArgs e)
        {
            canvaMenu.Visibility = Visibility.Hidden;
            canvaJouer.Visibility = Visibility.Visible;
        }

        // Boutons des cartes
        private void ButCarte1_Click(object sender, RoutedEventArgs e)
        {
            numCarte = "1";
            changementCarte = true;
            butCarte1.BorderBrush = Brushes.Green;
            butCarte2.BorderBrush = Brushes.Beige;
            butCarte3.BorderBrush = Brushes.Beige;
            ValidationBouton(butCarte1, butCarte2, butCarte3);
        }

        private void ButCarte2_Click(object sender, RoutedEventArgs e)
        {
            numCarte = "2";
            changementCarte = true;
            ValidationBouton(butCarte2, butCarte1, butCarte3);
        }

        private void ButCarte3_Click(object sender, RoutedEventArgs e)
        {
            numCarte = "3";
            changementCarte = true;
            ValidationBouton(butCarte3, butCarte2, butCarte1);
        }

        // Bouton retour à la première page du menu
        private void butRetour_Click(object sender, RoutedEventArgs e)
        {
            canvaMenu.Visibility = Visibility.Visible;
            canvaJouer.Visibility = Visibility.Hidden;
            canvaParametres.Visibility = Visibility.Hidden;
        }

        // Bouton paramètres
        private void butParametres_Click(object sender, RoutedEventArgs e)
        {
            canvaParametres.Visibility = Visibility.Visible;
            canvaMenu.Visibility = Visibility.Hidden;
        }

        // Bouton pour le changement de touches
        private void butTouche_Click(object sender, RoutedEventArgs e)
        {
            changementTouche = true;
            toucheChangee = false;
        }

        // Bouton pour lancer la MainWindow
        private void but2Jouer_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        // Bouton pour quitter le jeu
        private void butQuitter_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
