# Serious Game Design Document : GreenSkills

## 1. Titre et résumé

**Nom du projet :** GreenSkills

**Pitch :** GreenSkills est un jeu de gestion et de micro-apprentissage sur mobile destiné aux professionnels. Le joueur incarne un "Éco-Manager" chargé de transformer une entreprise traditionnelle en un modèle de durabilité. Via des sessions courtes de 2 minutes, le jeu transforme des concepts théoriques complexes en réflexes quotidiens.

## 2. Objectifs du Serious Game

### 2.1 Objectifs pédagogiques ou professionnels

- **Savoirs :** Comprendre les enjeux de la RSE (Responsabilité Sociétale des Entreprises) et de l'écologie.

- **Savoir-faire :** Acquérir des réflexes concrets tel que le tri des déchets, la suppression des données dormantes ou la configuration IoT.

- **Savoir-être :** Développer une conscience éthique et une prise de décision rapide face à des dilemmes comme par exemple la performance contre l'écologie.

### 2.2 Objectifs de gameplay

- **Complétion :** L'envie de "remplir la barre de progression", de valider tous les modules d'un chapitre et de débloquer les thématiques suivantes.

- **Maintien de la série :** Créer une habitude quotidienne forte. Le but ludique immédiat est de ne pas briser sa chaîne de jours consécutifs, créant une peur de la perte (le "Fear Of Missing Out") motivante.

- **Maîtrise et performance :** Obtenir le score parfait sur chaque micro-session pour maximiser les gains d'XP.

## 3. Public cible

### 3.1 Profil des utilisateurs

- **Cœur de cible :** Professionnels et salariés (industrie, bâtiment, tertiaire) et responsables RH/RSE.

- **Cible secondaire :** Étudiants en management ou écoles d'ingénieurs.

- **Niveau technologique :** Familiarisés avec les codes des applications mobiles modernes.


### 3.2 Contexte d'utilisation

- **Mobilité :** Utilisation sur smartphone durant les transports ou les pauses café.

- **Onboarding :** Intégré comme outil d'accueil pour les nouveaux employés en entreprise.

- **Formation continue :** Utilisé sur le long terme pour valider des acquis RSE.

## 4. Gameplay et mécaniques

### 4.1 Description du gameplay

- **Type de Jeu :** Jeu éducatif gamifié type quiz et puzzle rapide.

- **Métaphore :** L'arbre de compétences RSE. Au lieu de niveaux classiques, le joueur avance sur un chemin balisé par des thématiques.

- **Boucle de gameplay :**

    1. **Sélection :** Le joueur clique sur le prochain nœud actif de son parcours.

    2. **Exercices :** Enchaînement rapide de 5 à 10 micro-interactions variées :

        - _Texte à trous :_ Compléter une phrase sur la politique RSE.

        - _Vrai/Faux rapide :_ "Laisser un écran en veille consomme 0 énergie ?"

        - _Tri :_ Glisser les déchets dans la bonne poubelle.

        - _Mise en paire :_ Relier une action à son impact écologique.

    3. **Feedback immédiat :** Validation instantanée après chaque micro-interaction.

    4. **Récompense :** Fin de session avec score, gain d'XP, et progression visuelle de la barre de niveau.

### 4.2 Progression

- **Structure en "arbre" :**

    - Le jeu est découpé en **unités**, elles-mêmes divisées en **leçons**.

    - Le joueur ne peut pas accéder à l'unité 2 tant qu'il n'a pas validé l'unité 1.

- **Système de vies :**

    - Le joueur dispose de 5 "cœurs".

    - Chaque erreur dans une leçon coûte un cœur. Si le compteur tombe à zéro, le joueur doit attendre ou utiliser de la monnaie virtuelle pour en récupérer.

### 4.3 Gamification

- **Monnaie virtuelle (EcoCoins) :**

    - Gagnée à chaque leçon terminée.

    - Sert à acheter des "gels de série" pour protéger son streak un jour d'absence ou des cœurs de vie.

- **Défis Quotidiens :**

    - 3 objectifs aléatoires par jour pour donner une direction claire à la session de jeu.

### 4.4 Interactions

- **Interaction joueur-système :** Tout est tactile, rapide et satisfaisant. Cliquer, glisser, relier. Pas de longs textes à lire, l'interaction prime.

- **Interaction sociale :**

    - Pas de chat direct pour éviter la toxicité.

    - Possibilité d'envoyer des "félicitations" pré-enregistrées à un ami qui a passé un niveau ou battu un record.

- **Interaction narrative :**

    - De temps en temps, un personnage apparaît pour introduire une nouvelle notion ou faire une blague thématique, brisant la monotonie des exercices.

## 5. Narration et contexte

### 5.1 Synopsis

L'histoire n'est pas une longue cinématique, mais une **progression scénarisée** qui donne du sens à l'enchaînement des leçons.

- **Le pitch de départ :** Le joueur incarne une nouvelle recrue prometteuse, engagée en tant qu'**"éco-manager"** au sein d'une entreprise fictive caricaturale, vieillotte, énergivore, grise et triste.

- **La quête :** Sa mission est de transformer cette structure dépassée en un modèle de durabilité et de bien-être, en gravissant les échelons de la boîte.

- **Structure narratives :**

    - Le "chemin d'apprentissage" représente l'ascension du joueur dans l'entreprise.

    - Une fois toutes les leçons d'un département validées, le joueur "débloque" la zone : le décor se transforme.

### 5.2 Personnages

- **Le protagoniste :** Il est l'agent du changement, positif et dynamique.

- **Les Mentors :**

    - _Sam, l'expert RSE :_ Le guide principal. Il est bienveillant, explique les concepts clés avant les leçons et donne des astuces.

    - _Léa, la responsable Tech :_ Intervient spécifiquement sur les modules "Numérique responsable" et "Innovation".

- **Les "Obstacles" :**

    - Ils représentent les freins au changement en entreprise. Le but est de les convaincre, pas de les vaincre.

    - _M. Bernier, le directeur financier :_ Obsédé par les coûts. Il apparaît stressé quand on dépense du budget, mais ravi quand une action écolo fait faire des économies.

    - _Gérard, "l'ancien" :_ Réticent au changement. Il est bougon au début, mais devient votre plus grand fan une fois converti au tri sélectif.

- **Rôle dans le Gameplay :**

    - Ces personnages apparaissent en "pop-up" sur le côté de l'écran pendant les quiz pour donner un feedback émotionnel immédiat.
### 5.3 Univers

L'univers visuel est le principal indicateur de progression. C'est le **"miroir" des compétences** du joueur.

- **Lieu unique : Le siège social**

    - L'interface principale n'est pas une carte du monde, mais une vue du bâtiment de l'entreprise.

    - Au début, le bâtiment est gris, les fenêtres sont fermées, l'éclairage est au néon blafard, et les plantes sont fanées.

- **Évolution visuelle :**

    - À chaque **palier de compétence** validé, l'univers change visuellement :

        - _Niveau débutant :_ Les lumières deviennent LED, le tri est installé dans le hall.

        - _Niveau intermédiaire :_ Des plantes vertes envahissent l'open space.

        - _Niveau expert :_ Panneaux solaires sur le toit, potager partagé sur la terrasse, ambiance lumineuse et chaleureuse.

- **Ambiance sonore :**

    - _Départ :_ bruit de fond monotone.

    - _Progression :_ La musique devient plus acoustique et entraînante. Les bruits stressants laissent place à une ambiance feutrée et sereine.

## 6. Contenus et ressources

### 6.1 Compétences à mobiliser

Le jeu vise à transformer des concepts RSE théoriques en réflexes pratiques.

- **Savoirs :**

    - Compréhension des 3 piliers de la RSE.

    - Connaissance des règles de base de la sobriété numérique.

    - Identification des labels et certifications écologiques.

- **Savoir-faire :**

    - **Tri des déchets :** Savoir classer instantanément un déchet dans la bonne poubelle.

    - **Optimisation énergétique :** Identifier les sources de gaspillage dans un bureau et agir dessus.

    - **Priorisation :** Choisir l'action ayant le meilleur ratio Impact/Effort dans un temps limité.

- **Savoir-Être :**

    - **Réflexe éthique :** Prendre une décision rapide face à un dilemme moral.

    - **Bienveillance :** Adopter une communication positive pour inciter au changement.

### 6.2 Contenus pédagogiques

Pour le MVP, le contenu est structuré pour être léger, modulaire et facile à produire.

- **La base de questions :**

    - Cœur du jeu. Un fichier structuré contenant 50 à 100 micro-interactions pour le lancement.

    - **Types de contenus :**

        - _Quiz Vrai/Faux_.

        - _QCM_.

        - _Associations_.

- **Fiches "Le saviez-vous ?" :**

    - Courtes anecdotes ou chiffres clés affichés pendant les écrans de chargement ou après une réponse correcte pour renforcer la mémorisation.

- **Feedbacks correctifs :**

    - Chaque mauvaise réponse déclenche une "pop-up pédagogique" : une explication de 2 phrases max pour corriger l'idée reçue instantanément, sans blâmer le joueur.

### 6.3 Assets nécessaires

Liste des ressources graphiques et sonores à produire pour le MVP Unity.

- **Graphismes :**

    - **Le Bureau évolutif :**

        - _État 1 (Départ) :_ Bureau gris, désordonné, lumière froide.

        - _État 2 (Intermédiaire) :_ Bureau rangé, poubelles de tri visibles.

        - _État 3 (Final) :_ Plantes vertes, lumière chaude, panneaux solaires.

    - **Interface utilisateur :**

        - Icônes des thématiques.

        - Jauges.

        - Badges et trophées.

- **Audio :**

    - **UI sounds :**

        - _Validation :_ Son cristallin/positif.

        - _Erreur :_ Son sourd mais doux.

        - _Level Up :_ Jingle court et motivant.

    - **Ambiance :**

        - Une boucle musicale unique, calme et propice à la concentration, pour ne pas fatiguer l'utilisateur lors de sessions répétées.

## 7. Technologie et plateformes

### 7.1 Moteur de Jeu

**Choix technologique : Unity**

- **Justification :** Unity est particulièrement adapté pour gérer l'interface utilisateur complexe et réactive nécessaire à un jeu de type "Gamified Learning".

- **Architecture :**

    - Utilisation du **2D Render Pipeline** (URP) pour optimiser les performances sur mobile et garantir une fluidité parfaite même sur des smartphones de milieu de gamme.

### 7.2 Plateformes cibles

**Cœur de cible : Mobile (iOS et Android)**

- **Format :** Application native optimisée pour une utilisation verticale, adaptée à l'usage "à une main" dans les transports ou lors de courtes pauses.

- **Distribution :** Déploiement via Apple App Store et Google Play Store.

- **Compatibilité :** L'objectif est de couvrir le parc de smartphones professionnels standard pour maximiser l'accessibilité en entreprise.


### 7.3 Langues disponibles

**Stratégie de localisation**

- **Lancement :** **Français**. Le jeu sera initialement déployé pour le marché francophone pour valider les contenus pédagogiques et l'engagement.

- **Évolution :** **Anglais**, suivi d'autres langues selon les besoins des clients internationaux.

- **Implémentation technique :** Architecture dès le départ conçue pour l'internationalisation. Tous les textes sont externalisés dans des fichiers de clés de traduction ou récupérés via l'API, permettant d'ajouter une nouvelle langue côté serveur sans modifier le code source du jeu.

### 7.4 Technologies spécifiques

- **API et Backend : Go**

    - **Performance :** Go est choisi pour sa capacité à gérer la concurrence avec les Goroutines. C'est idéal pour supporter des pics de charge où des milliers de collaborateurs se connectent simultanément pour leur session de 2 minutes.

    - **Rôle :** L'API gère la logique métier critique : validation des réponses aux quiz, calcul des points, gestion des classements et distribution du contenu pédagogique.

- **Base de données : PostgreSQL**

    - **Structure :** Base de données relationnelle robuste pour structurer les données complexes de l'arbre de compétences.

    - **Intégrité :** Garantit la fiabilité des données de formation et la persistance des états de jeu.

- **Communication client-serveur :**

    - **RESTful API / JSON :** Échanges de données légers pour minimiser la consommation de data mobile des utilisateurs.

    - **Sécurité :** Authentification sécurisée via JSON Web Tokens pour protéger les comptes utilisateurs et l'accès aux contenus d'entreprise.

## 8. Système de feedback

### 8.1 Évaluation des joueurs

- **Système de "maîtrise" :**

    - Chaque leçon est évaluée sur une échelle de **1 à 3 étoiles** selon la performance et le nombre d'erreurs commises.

    - **Niveaux de compétence :** Le joueur progresse de "novice" à "expert" sur des thématiques précises.

- **Tableaux de bord :**

    - **Vue joueur :** L'arbre de compétences sert de tableau de bord visuel. Les nœuds changent de couleur pour indiquer l'acquisition des savoirs.

### 8.2 Retours immédiats

- **Feedback sensoriel :**

    - **Visuel :** Animations "Juicy" lors d'une bonne réponse. En cas d'erreur, l'élément tremble en rouge.

    - **Sonore :** Sons distinctifs et satisfaisants pour valider l'apprentissage ou signal sonore doux pour une erreur.

- **Correction pédagogique Instantanée :**

    - Contrairement à un examen, l'erreur est immédiatement corrigée.

    - Une fenêtre pop-up apparaît juste après une mauvaise réponse pour expliquer _pourquoi_ c'est faux, transformant l'erreur en opportunité d'apprentissage.

- **Jauge de "cœurs" :**

    - Indicateur visuel en haut de l'écran. Chaque erreur brise un cœur. Cela matérialise le "droit à l'erreur" limité et encourage la concentration.

### 8.3 Suivi post-jeu

- **Écran de fin de session :**

    - Résumé immédiat affiché après les 2 minutes de jeu :

        - **Score :** XP gagnés et EcoCoins récoltés.

        - **Précision :** Pourcentage de bonnes réponses.

        - **Série :** Animation de la série qui s'active pour la journée.

- **Rapports pour l'entreprise :**

    - Grâce à l'API et la base de données, les données sont agrégées pour les responsables de formation.

    - **Export des données :** Génération de fichiers ou visualisation web listant les compétences validées par les employés.

- **Ancrage mémoriel :**

    - Le système analyse les leçons terminées depuis longtemps et propose des notifications "révision" pour consolider les acquis avant qu'ils ne soient oubliés.

## 9. Contraintes et faisabilité

### 9.1 Durée de développement

**Stratégie :** Mode "Sprint". L'objectif est de sortir le cœur du jeu sans fioritures. 
**Durée totale :** 8 à 9 Semaines.

- **Phase 1 : Conception et setup (semaines 1-2)**

    - **Focus :** Ne rien inventer en cours de route.

    - **Actions :**

        - Figer strictement le périmètre : 1 seule thématique de quiz, 1 seul type de décor, pas de personnalisation d'avatar complexe.

        - Setup technique immédiat : Initialisation du projet Unity et de la base de données PostgreSQL.

        - Création des Wireframes uniquement pour les 3 écrans clés : Accueil, Quiz, Résultat.

- **Phase 2 : Production "core" (semaines 3-7)**

    - **Focus :** Le "Gameplay" avant le visuel.

    - **Actions :**

        - Intégration de la mécanique de base et de la navigation.

        - API minimale pour l'authentification et la sauvegarde du score.

        - Création des assets UI essentiels et d'un seul état visuel du bureau.

- **Phase 3 : Stabilisation et livraison (semaine 8)**

    - **Focus :** Zéro bug bloquant.

    - **Actions :**

        - Tests intensifs sur 2-3 appareils de référence.

        - Correction des bugs critiques.

        - Déploiement sur les stores et validation.

### 9.2 Budget estimé

**Nouvelle Fourchette :** 35 000 €.

- **Poste de dépense principal :** un développeur (Unity + Backend) à temps plein sur 2 mois.

- **Économie :** On reporte les tests utilisateurs, la traduction et le marketing à la phase "Post-MVP".

### 9.3 Ressources nécessaires


- **1 Développeur Fullstack :** Capable de coder vite et propre.

- **1 Game designer :** Pour trancher les décisions en moins de 24h et fournir les contenus immédiatement.

- **1 Graphiste UI :** Pour donner un look professionnel à l'interface sans produire des centaines d'assets.

### 9.4 Risques identifiés

- **Le "Feature Creep" :**

    - _Risque :_ Vouloir rajouter "juste une petite fonctionnalité" en semaine 4.

    - _Conséquence :_ Le projet ne sort pas à temps.

    - _Solution :_ Tout ce qui n'est pas dans le doc initial est refusé et noté pour la "V2".

- **Validation des stores :**

    - _Risque :_ Apple et Google peuvent prendre 1 semaine pour valider l'app.

    - _Solution :_ Soumettre une version très basique dès la semaine 6 pour pré-valider le compte développeur.

## 10. Plan de lancement et évaluation

### 10.1 Stratégie de déploiement

Compte tenu du délai court, la stratégie privilégie un **"Soft Launch"** plutôt qu'une grande sortie publique, afin de valider la stabilité de l'infrastructure auto-hébergée.

- **Infrastructure Backend :**

    - **Conteneurisation :** L'API et la base de données seront dockerisées.

    - **Orchestration via Dokploy :** Utilisation de Dokploy sur un VPS pour gérer le déploiement continu.

        - _Avantage :_ Mise en production automatique des correctifs API sans interruption de service majeure pour les testeurs.

        - _Sécurité :_ Gestion des certificats SSL via Traefik pour sécuriser les échanges Mobile et VPS.

    - **Sauvegardes :** Configuration de backups automatisés de la base de données via l'interface Dokploy.

- **Distribution client mobile :**

    - **Android :** Génération d'un fichier `.apk` distribué directement aux testeurs via un lien de téléchargement sécurisé.

    - **iOS :** Utilisation de _TestFlight_ ou limitation initiale aux appareils Android pour le MVP afin d'éviter les délais de validation Apple.

### 10.2 Évaluation de l'impact

Pour ce premier lancement, on ne cherche pas la rentabilité, mais la **validation technique et d'usage**.

- **Indicateurs techniques :**

    - **Uptime VPS :** Disponibilité de l'API.

    - **Temps de réponse :** Latence moyenne des requêtes API.

    - **Taux de crash :** Nombre de fermetures inopinées de l'application Unity.

- **Indicateurs d'engagement :**

    - **Taux de complétion :** Pourcentage d'utilisateurs ayant terminé la première "unité".

    - **Rétention J+3 :** Pourcentage d'utilisateurs qui reviennent jouer 3 jours après l'installation.

- **Retours qualitatifs :**

    - Mise en place d'un formulaire simple pour remonter les bugs visuels ou les questions mal formulées.

### 10.3 Mises à jour et maintenance

Le cycle de mise à jour post-lancement est conçu pour être agile, grâce à l'architecture découplée.

- **Correctifs critiques :**

    - **Backend :** Grâce à Dokploy, les bugs logiques peuvent être corrigés et déployés en quelques minutes sans que l'utilisateur ait besoin de mettre à jour son application mobile.

    - **Contenu :** Les questions étant servies par l'API, une faute d'orthographe ou une erreur RSE peut être corrigée instantanément côté serveur.

- **Évolutions :**

    - Si le MVP est validé, les mises à jour suivantes se concentreront sur :

        1. L'ajout de contenu avec de nouvelles thématiques RSE.

        2. L'amélioration visuelle.

        3. Le portage iOS officiel sur l'App Store.

## 11. Annexes

### 11.1 Références

- **Références de gameplay :**

    - **Duolingo :** Pour la structure en arbre de compétences, le format de micro-leçons, le système de vies et la mécanique de Streak.

    - **Tinder :** Pour la mécanique de tri rapide utilisée dans les mini-jeux de tri des déchets ou d'emails.

- **Références visuelles :**

    - **Isometric Office / Tiny Tower :** Pour la vue en coupe du bâtiment et l'évolution visuelle des bureaux.

    - **Flat Design "Corporate" :** Style graphique épuré, utilisant des aplats de couleurs douces pour rester lisible sur mobile.

- **Références pédagogiques :**

    - **ISO 26000 :** Lignes directrices relatives à la responsabilité sociétale.

    - **ADEME :** Guides sur l'éco-responsabilité au bureau et la sobriété numérique.

### 11.2 Wireframes ou mockups

- **Écran 1 : Le Hub**

![image](mockups/hub.jpg)

- _Vue principale :_ Vue isométrique du bureau actuel du joueur.

- _Zone centrale :_ Le décor qui change selon le niveau.

- _Overlay :_ Le chemin de progression avec des nœuds cliquables.

- _Header :_ Jauge de niveau, compteur de cœurs, indicateur de série.

- _Footer :_ Boutons de navigation.

- **Écran 2 : Le jeu**

![image](mockups/question.jpg)

- _Zone supérieure :_ Barre de progression de la leçon en cours et bouton "pause".

- _Zone centrale :_ L'élément interactif.

- _Zone inférieure :_ Boutons de réponse ou zone de drop pour le tri.

- _Feedback :_ Un personnage apparaît brièvement sur le côté pour valider ou corriger.

- **Écran 3 : Résultat**

![image](mockups/victory.jpg)

- _Visuel :_ Animation de coffre ou de confettis.

- _Données :_ Score obtenu, gain d'XP, confirmation de la série journalière.

- _Call-to-Action :_ Bouton "continuer" ou "rejouer".

### 11.3 Liste des besoins techniques

Inventaire du matériel et des services nécessaires pour le déploiement de l'infrastructure Dockerisée.

- **Hébergement et infrastructure :**

    - **VPS :**

        - _Config recommandée MVP :_ 2 vCPU, 4 Go RAM, 40 Go SSD.

        - _OS :_ Ubuntu 20.04 ou 22.04 LTS.

    - **Orchestration :** Dokploy installé sur le VPS.

    - **Domaine :** Un nom de domaine pour gérer les certificats SSL via Traefik.

- **Outils de développement :**

    - **Moteur :** Unity.

    - **IDE :** Rider et GoLand.

- **Services tiers :**

    - **Versionning :** GitHub pour héberger le code source.

    - **Google Play Console :** Compte développeur pour distribuer l'APK ou publier la bêta.
