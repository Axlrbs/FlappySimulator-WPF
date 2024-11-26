using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Media;
using System.Numerics;
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
using System.Windows.Threading;

namespace Jeu_SAe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Instance de la classe MenuAccueil afin d'accéder aux champs et aux méthodes de celle ci
        MenuAccueil menu = new MenuAccueil();

        public DispatcherTimer minuterieJeu = new DispatcherTimer();

        // création de la hitBox des différents objets sur la map
        private Rect joueurHitbox;
        private Rect obstacleHitbox;

        // déclaration des images
        private ImageBrush joueurMouvement = new ImageBrush();
        private ImageBrush imgObstacle = new ImageBrush();

        // sond quand le joueur meurt
        private SoundPlayer gameOverSound = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + "sound\\gameOverSound.wav");

        // variable pour le temps du saut
        private double tempsDepuisSaut = 0;

        // double pour le score
        private double score = 0;
        private double meilleurScore = 0;
        
        // variables pour la distance min et la distance max entre deux obstacles
        private int distanceMin = 0;
        private int distanceMax = 0;

        // liste qui permet de gérer les obstacles
        List<Rectangle> listeObstacles = new List<Rectangle>();
        List<Rectangle> obstaclesAEnlever = new List<Rectangle>();

        // Création d'une variable permettant d'utiliser une méthode à un intervalle précis
        private int temps;

        // variable afin de modifier l'image du joueur
        private int indexJoueur = 1;

        // booléen pour la fin du jeu
        private bool finDuJeu = false;

        // constantes
        private readonly int NB_PIXEL_PAR_RAFFRAICHISSEMENT = 8;
        private readonly double GRAVITE = 9.8;
        private readonly int POSITION_DEBUT_JOUEUR = 100;
        private readonly double TEMPS_CREATION_OBSTACLE = 30;
        private readonly int HAUTEUR_MAX_ECRAN_HITBOX = 0;
        private readonly int HAUTEUR_MIN_ECRAN_HITBOX = 320;
        private readonly int VALEUR_HB_HAUTEUR_JOUEUR = 6;
        private readonly int VALEUR_HB_LARGEUR_JOUEUR = 12;
        private readonly int DEGRE_RENVERSEMENT_OBSTACLE = 180;
        private readonly int DEGRE_RENVERSEMENT_JOUEUR = 5;


        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            Console.WriteLine("Debug VERSION");
#endif

            menu.ShowDialog();

            if (menu.DialogResult == false)
                Application.Current.Shutdown();

            MyCanva.Focus();
            // assigne le moteur de jeu avec la minuterie du jeu
            minuterieJeu.Tick += MoteurdeJeu;
            minuterieJeu.Interval = TimeSpan.FromMilliseconds(20);

            fond1.Fill = menu.fond1.Fill;
            fond2.Fill = menu.fond2.Fill;
            fond3.Fill = menu.fond3.Fill;
            sol1.Fill = menu.sol1.Fill;
            sol2.Fill = menu.sol2.Fill;
            sol3.Fill = menu.sol3.Fill;
#if DEBUG
            Console.WriteLine("Debug version");
#endif

            switch (menu.difficulteChoisie)
            {
                case "Facile":
                    menu.vitesseFond = 1;
                    menu.vitesseSol = 5;
                    break;
                case "Moyen":
                    menu.vitesseFond = 3;
                    menu.vitesseSol = 7;
                    break;
                case "Difficile":
                    menu.vitesseFond = 5;
                    menu.vitesseSol = 9;
                    break;
            }
#if DEBUG 
            Console.WriteLine("Vitesse fond : " + menu.vitesseFond);
            Console.WriteLine("Vitesse sol : " + menu.vitesseSol);
#endif
            temps = 0;
            CreationObstacle();
            StartGame();

            imgObstacle.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "images/armoire.png"));
        }

        private void StartGame()
        {
            score = 0;
            tempsDepuisSaut = 0;

            Canvas.SetTop(joueur, POSITION_DEBUT_JOUEUR);

            EffetMouvementVol(indexJoueur);
            finDuJeu = false;

            TexteRejouer.Visibility = Visibility.Hidden;
            boutonQuitterPartie.Visibility = Visibility.Hidden;

            menu.backgroundSound.PlayLooping();

            minuterieJeu.Start();

            // Efface tous les obstacles du Canva, présent dans la liste d'obstacles
            foreach (Rectangle obstacle in listeObstacles)
            {
                MyCanva.Children.Remove(obstacle);
            }
            listeObstacles.Clear();
            obstaclesAEnlever.Clear();
        }

        private void MoteurdeJeu(object? sender, EventArgs e)
        {
            menu.MouvementFond(fond1, fond2, fond3);
            menu.MouvementSol(sol1, sol2, sol3);

            temps++;
            // Créer un obstacle environ toute 0.5 seconde
            if (temps == TEMPS_CREATION_OBSTACLE)
            {
#if DEBUG 
                Console.WriteLine("Obstacles créés");
#endif
                CreationObstacle();
                temps = 0;
            }

            foreach (Rectangle obstacle in listeObstacles)
            {
                MouvementObstacle(obstacle);
            }


            // applique la hitbox du joueur sur le joueur
            joueurHitbox = new Rect(Canvas.GetLeft(joueur), Canvas.GetTop(joueur), joueur.Width - VALEUR_HB_LARGEUR_JOUEUR, joueur.Height - VALEUR_HB_HAUTEUR_JOUEUR);

            DeplacementJoueur();
            tempsDepuisSaut += 0.1; // Ajoute 0,1 au temps de saut pour que la formule mathématique augmente

            // teste si le joueur sort de la taille de l'écran 
            if ( Canvas.GetTop(joueur) < HAUTEUR_MAX_ECRAN_HITBOX || Canvas.GetTop(joueur) > HAUTEUR_MIN_ECRAN_HITBOX)
            {
                TexteRejouer.Visibility = Visibility.Visible;
                boutonQuitterPartie.Visibility = Visibility.Visible;
                meilleurScore = ScoreMax(score, meilleurScore);
                finDuJeu = true;
                gameOverSound.Play();
                minuterieJeu.Stop();
#if DEBUG
                Console.WriteLine("Joueur Mort");
                Console.WriteLine("Nb obstacle sur le canva :" + listeObstacles.Count);
                Console.WriteLine("Nb obstacle en dehors du canva : " + obstaclesAEnlever.Count);
#endif
            }

            if (indexJoueur > 2)
                indexJoueur = 1;
            EffetMouvementVol(indexJoueur);

            foreach (Rectangle obstacle in listeObstacles)
            {
                TesteCollisionJoueur(obstacle);
            }
            EnleverObstacle();


        }

        // Les 3 prochaines méthodes concernent le joueur...
        // Cette méthode permet de modéliser la vitesse du joueur
        private double CalculeVitesseJoueur() { return GRAVITE * tempsDepuisSaut - NB_PIXEL_PAR_RAFFRAICHISSEMENT; }

        private void DeplacementJoueur()
        {
            // Cette méthode permet de déplacer le joueur à partir de la méthode CalculeVitesseJoueur()
            double vitesseJoueur = CalculeVitesseJoueur();
#if DEBUG
            Console.WriteLine("Vitesse du joueur " + vitesseJoueur);
#endif
            Canvas.SetTop(joueur, Canvas.GetTop(joueur) + vitesseJoueur);
        }

        private void TesteCollisionJoueur(Rectangle obstacle)
        {

            double yHitbox;
            // applique la hitbox des obstacles 
            if (obstacle.Name == "obstacleBas")
                yHitbox = Canvas.GetTop(obstacle) + 10; // Adaptation de l'ordonnée de la hitbox de l'obstacle en la réduisant
            else
                yHitbox = -Canvas.GetBottom(obstacle) - 50; // Idem
            obstacleHitbox = new Rect(Canvas.GetLeft(obstacle), yHitbox, obstacle.Width, obstacle.Height);
#if DEBUG
            Console.WriteLine("Ordonnée hitbow obstacle : " + yHitbox);
#endif



            // teste si la hitbox du joueur touche la hitbox de l'obstacle
            if (joueurHitbox.IntersectsWith(obstacleHitbox))
            {
                minuterieJeu.Stop();
                finDuJeu = true;
                TexteRejouer.Visibility = Visibility.Visible;
                boutonQuitterPartie.Visibility = Visibility.Visible;
                meilleurScore = ScoreMax(score, meilleurScore);
                gameOverSound.Play();
#if DEBUG
                Console.WriteLine("Length listeObstacle :" + listeObstacles.Count);
                Console.WriteLine("Length listeAvider :" + obstaclesAEnlever.Count);
#endif
            }
            MiseaJourScore(obstacle);
            TexteScore.Content = "Score : " + score;
            MeilleurScore.Content = "Meilleur score : " + meilleurScore;
        }

        // Les 3 prochaines méthodes concernent les obstacles...
        private void CreationObstacle()
        {
            // Cette méthode permet de créer des obstacles
            Random random = new Random();

            // Donne un nombre aléatoire entre 190 et 299 donnant la position haut de l'obstacle
            int topAleatoire = random.Next(190, 300);

            // Création de l'obstacle du bas
            Rectangle obstacleBas = new Rectangle()
            {
                Name = "obstacleBas",
                Width = 60,
                Height = 390,
                Fill = Brushes.Black,
                Tag = "Obstacle",
            };
            
            Canvas.SetTop(obstacleBas, topAleatoire);
            Canvas.SetLeft(obstacleBas, MyCanva.ActualWidth);

            switch (menu.difficulteChoisie)
            {
                case "Facile":
                    distanceMin = 90;
                    distanceMax = 140;
                    break;
                case "Moyen":
                    distanceMin = 60;
                    distanceMax = 90;
                    break;
                case "Difficile":
                    distanceMin = 30;
                    distanceMax = 60;
                    break;
            }
            // Definit une distance aléatoire entre l'obstacle du bas et l'obstacle du haut
            int distanceAleatoire = random.Next(distanceMin, distanceMax);

            //Création de l'obstacle du haut
            Rectangle obstacleHaut = new Rectangle()
            {
                Name = "obstacleHaut",
                Width = obstacleBas.Width,
                Height = obstacleBas.Height,
                Fill = Brushes.Black,
                Tag = "Obstacle"
            };
            Canvas.SetBottom(obstacleHaut, MyCanva.ActualHeight - Canvas.GetTop(obstacleBas) + distanceAleatoire);
            Canvas.SetLeft(obstacleHaut, Canvas.GetLeft(obstacleBas));

            obstacleHaut.RenderTransform = new RotateTransform(DEGRE_RENVERSEMENT_OBSTACLE, obstacleHaut.Width / 2, obstacleHaut.Height / 2);

            
            obstacleBas.Fill = imgObstacle;
            obstacleHaut.Fill = imgObstacle;

            MyCanva.Children.Add(obstacleBas);
            MyCanva.Children.Add(obstacleHaut);

            listeObstacles.Add(obstacleBas);
            listeObstacles.Add(obstacleHaut);

#if DEBUG 
            Console.WriteLine("longueur listeObstacle " + listeObstacles.Count);
            Console.WriteLine("longueur liste a vider : " + obstaclesAEnlever.Count);
#endif
        }

        private void MouvementObstacle(Rectangle obstacle)
        {
            // Cette méthode permet de déplacer les obstacles
            Canvas.SetLeft(obstacle, Canvas.GetLeft(obstacle) - menu.vitesseSol);

            if (Canvas.GetLeft(obstacle) < -obstacle.Width)
            {
                obstaclesAEnlever.Add(obstacle);
            }
        }

        private void EnleverObstacle()
        {
            // Cette méthode permet de supprimer les obsctales une fois sorties de l'écran
            foreach (Rectangle obstacle in obstaclesAEnlever)
            {
                listeObstacles.Remove(obstacle);
                MyCanva.Children.Remove(obstacle);
            }
            obstaclesAEnlever.Clear();
        }

        // Les 3 prochaines méthodes concernent l'affichage des élements sur le canva...
        private void MiseaJourScore(Rectangle x)
        {
            // Cette méthode permet de mettre à jour le score une fois l'osbstacle passé par le joueur
            if ((string)x.Tag != "Passé")
            {
                // Cette méthode mets à jour le score quand le joueur passe l'obstacle
                if (Canvas.GetLeft(x) <= Canvas.GetLeft(joueur) - x.Width)
                {
                    score += .5;
                    x.Tag = "Passé";
                }
#if DEBUG
                Console.WriteLine("Tag obstacle : " + (string)x.Tag);
#endif
            }
        }

        private static double ScoreMax(double score, double meilleurScore)
        {
            // Cette méthode permet de garder le score Max durant une session
            // renvoie le score Max durant une session
            if (score >= meilleurScore)
                return score;
            return meilleurScore;
        }

        private void EffetMouvementVol(double index)
        {
            // Cette méthode permet de donner un effet de mouvement au player
            switch (index)
            {
                case 1:
                    joueurMouvement.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "images/caractereVille/frame-2.gif"));
                    break;
                case 2:
                    joueurMouvement.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "images/caractereVille/frame-3.gif"));
                    break;
            }
            joueur.Fill = joueurMouvement;
        }

    
        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            // Cette méthode est utilisé quand une touche est enfoncée
            if (e.Key == menu.touche)
            {
                // rotation du joueur de -20 degré par rapport au centre de la position
                joueur.RenderTransform = new RotateTransform(-20, joueur.Width / 2, joueur.Height / 2);
                tempsDepuisSaut = 0;
                indexJoueur++;
#if DEBUG
                Console.WriteLine("Index joueur mouvement : " + indexJoueur);
#endif
            }
            
            if (e.Key == Key.R && finDuJeu == true)
            {
                StartGame();
            }
        }

        private void Canvas_KeyUp(object sender, KeyEventArgs e)
        {
            // Cette méthode est utilisé lorsque le joueur relache la touche de saut
            joueur.RenderTransform = new RotateTransform(DEGRE_RENVERSEMENT_JOUEUR, joueur.Width / 2, joueur.Height / 2);
        }

        // Bouton pour quitter le jeu
        private void boutonQuitterPartie_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}