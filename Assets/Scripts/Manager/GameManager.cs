using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Constants.
    private const float LEVEL_INFO_FLASH_TIME = 3.0f;

    // Public member variables.
    public static bool showBothScores;
    public static float player1Score;
    public static float player2Score;

    // Stuff for the players.
    public Sprite player1LancerSprite;
    public Sprite player1VanguardSprite;
    public Sprite player1TrailblazerSprite;

    public Sprite player2LancerSprite;
    public Sprite player2VanguardSprite;
    public Sprite player2TrailblazerSprite;

    public GameObject player1LancerUlt;
    public GameObject player1VanguardUlt;
    public GameObject player1TrailblazerUlt;
    public GameObject player2LancerUlt;
    public GameObject player2VanguardUlt;
    public GameObject player2TrailblazerUlt;

    public InventoryBehavior player1Inventory;
    public InventoryBehavior player2Inventory;

    // Stuff for levels.
    public ItemSelection itemSelection;
    public GameObject levelAttach;
    public EnemySpawnControl spawner;

    public TMPro.TextMeshProUGUI levelNum;
    public TMPro.TextMeshProUGUI levelName;
    public TMPro.TextMeshProUGUI pausedText;

    public static bool winLoss;

    // Private member variables.
    private PlayerBehavior player1;
    private PlayerBehavior player2;

    private Vector3 player1StartPos = new Vector3(-1.5f, -2.5f, 0.0f);
    private Vector3 player2StartPos = new Vector3(1.5f, -2.5f, 0.0f);

    private bool ready;
    private bool singlePlayer;
    private bool bossAlive;
    private bool endless;
    private bool paused;

    private int levelNumber;
    private float TimeBetweenSpawns;

    private float levelTime;

    private string[] levelNames;

    // Start is called before the first frame update
    void Start()
    {
        // Perform some checks.
        Debug.Log("GameManager: Waking up!");
        Debug.Assert(itemSelection != null);

        // Make sure we have valid references to our inventories.
        Debug.Assert(player1Inventory != null);
        Debug.Assert(player2Inventory != null);

        // Build the players.
        BuildPlayers(MainMenu.player1Ship, MainMenu.player2Ship);

        // Set up the inventories.
        player1Inventory.SetPlayer(player1);
        if (player2 != null)
        {
            player2Inventory.SetPlayer(player2);
        }

        // This takes care of the levels - the actual gameplay.
        spawner = levelAttach.GetComponent<EnemySpawnControl>();
        StartCoroutine(StartGame());

        // This tells the ScoreManager we've determined whether this is single player.
        ready = true;

        winLoss = false;

        endless = false;

        paused = false;

        TimeBetweenSpawns = 3f;

        levelNumber = 1;

        levelNames = new string[] {"You Asked For This", "Endless Space", 
                                    "Unlimited Ship Works", "Paradise Lost", 
                                    "Inferno", "Literary Allusion", 
                                    "Inherit the Stars", "No Longer Human", 
                                    "Childhood's End", "(Don't Fear) The Reaper",
                                    "Something About A Windmill", "Apocalypse",
                                    "Back To The Beginning", "Endless Mode (Wait did we use that already)",
                                    "Star War", "%&@!*($", "Infinite Cosmos",
                                    "Nebulaic Formula", "Black Horizon", "C-C-Combo!"};
    }

    // Update is called once per frame
    void Update()
    {
        DetectCondition();

        // Pseudo-invulnerability key
        if (Input.GetKeyDown("n"))
        {
            GameObject[] pl = GameObject.FindGameObjectsWithTag("Player");
            foreach(GameObject p in pl)
            {
                p.transform.position = new Vector3(0.0f, -999.0f, 0.0f);
            }
        }
        if (Input.GetKeyUp("n"))
        {
            GameObject[] pl = GameObject.FindGameObjectsWithTag("Player");
            foreach(GameObject p in pl)
            {
                p.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }

        if (Input.GetKeyDown("j") && levelNumber <= 4)
        {
            endless = !endless;  
            Debug.Log(endless);
        }

        if (Input.GetKeyDown("p"))
        {
            if (paused == false)
            {
                pausedText.enabled = true;
                PauseGame();
                Debug.Log("Paused");
                paused = true;
            }
            else
            {
                pausedText.enabled = false;
                ResumeGame();
                Debug.Log("Unpausing");
                paused = false;
            }
        }

        if (Input.GetKeyDown("k"))
        {
            Debug.Log("PLAYER DIED");
            winLoss = false;
            showBothScores = singlePlayer;
            player1Score = GetComponent<ScoreManager>().GetPlayer1Score();
            player2Score = GetComponent<ScoreManager>().GetPlayer2Score();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    // Public methods.
    public bool Ready()
    {
        return ready;
    }

    public bool SinglePlayer()
    {
        return singlePlayer;
    }

    public PlayerBehavior GetPlayer1()
    {
        return player1;
    }

    public PlayerBehavior GetPlayer2()
    {
        return player2;
    }

    // Private helper methods.
    private void BuildPlayers(string ship1, string ship2)
    {
        Quaternion rotation = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
        if ((ship1 != null && ship1 != "None") && (ship2 == null || ship2 == "None"))
        {
            Debug.Log("GameManager: Singleplayer detected");
            player1StartPos = new Vector3(0.0f, -2.5f, 0.0f);
            player2Inventory.Hide();
            singlePlayer = true;
        }

        for (int i = 0; i < 2; i++)
        {
            bool playerOne = i == 0;
            string ship = playerOne ? ship1 : ship2;

            if (ship != null && ship != "None")
            {
                // Instantiate the game object.
                Vector3 startPos = playerOne ? player1StartPos : player2StartPos;
                GameObject player = Instantiate(Resources.Load("Prefabs/Player") as GameObject,
                                                    startPos,
                                                    rotation);
                player.tag = "Player";

                // Set up the behavior component.
                PlayerBehavior playerBehavior = player.GetComponent<PlayerBehavior>();
                playerBehavior.playerOne = playerOne;
                playerBehavior.SetShipName(ship);
                playerBehavior.SetInventory(playerOne ? player1Inventory : player2Inventory);

                if (playerOne)
                {
                    player1 = playerBehavior;
                }
                else
                {
                    player2 = playerBehavior;
                }

                // Set the sprite, camera shake, and add appropriate components.
                SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
                if (ship == "Lancer")
                {
                    renderer.sprite = playerOne ? player1LancerSprite : player2LancerSprite;

                    // Set stats.
                    playerBehavior.GetHealthBar().SetHitPoints(150.0f);
                    playerBehavior.SetWeaponDamage(25.0f);
                    playerBehavior.SetInitialSpeed(5.0f);

                    // Add appropriate components.
                    player.AddComponent<BaseMovement>();
                    player.GetComponent<BaseMovement>().SetParent(playerBehavior);

                    player.AddComponent<BaseCollider>();
                    player.GetComponent<BaseCollider>().SetParent(playerBehavior);

                    player.AddComponent<BaseWeapon>();
                    player.GetComponent<BaseWeapon>().SetParent(playerBehavior);
                    player.GetComponent<BaseWeapon>().SetFireRate(.4f);

                    player.AddComponent<LancerBasicAbility>();
                    player.GetComponent<LancerBasicAbility>().SetParent(playerBehavior);

                    player.AddComponent<LancerUltimateAbility>();
                    player.GetComponent<LancerUltimateAbility>().SetParent(playerBehavior);
                    SetupUltBars(playerOne, ship);
                }
                else if (ship == "Vanguard")
                {
                    renderer.sprite = playerOne ? player1VanguardSprite : player2VanguardSprite;
                    player.transform.localScale = new Vector3(1.5f, 1.5f, 1);

                    playerBehavior.GetHealthBar().SetHitPoints(200.0f);
                    playerBehavior.SetWeaponDamage(20.0f);
                    playerBehavior.SetInitialSpeed(3.0f);

                    player.AddComponent<VanguardMovement>();
                    player.GetComponent<VanguardMovement>().SetParent(playerBehavior);

                    player.AddComponent<VanguardCollider>();
                    player.GetComponent<VanguardCollider>().SetParent(playerBehavior);

                    player.AddComponent<VanguardUltimate>();
                    player.GetComponent<VanguardUltimate>().SetParent(playerBehavior);

                    player.AddComponent<BaseWeapon>();
                    player.GetComponent<BaseWeapon>().SetParent(playerBehavior);
                    player.GetComponent<BaseWeapon>().SetFireRate(.5f);
                    SetupUltBars(playerOne, ship);
                }
                else if (ship == "Trailblazer")
                {
                    // Otherwise, given the string is not null, it must be the trailblazer.
                    renderer.sprite = playerOne ? player1TrailblazerSprite : player2TrailblazerSprite;
                    player.transform.localScale = new Vector3(.75f, .75f, 1);

                    playerBehavior.GetHealthBar().SetHitPoints(100.0f);
                    playerBehavior.SetWeaponDamage(20.0f);
                    playerBehavior.SetInitialSpeed(7.0f);

                    // Add appropriate components
                    player.AddComponent<TrailblazerMovement>();
                    player.GetComponent<TrailblazerMovement>().SetParent(playerBehavior);

                    player.AddComponent<TrailblazerCollider>();
                    player.GetComponent<TrailblazerCollider>().SetParent(playerBehavior);

                    player.AddComponent<BaseWeapon>();
                    player.GetComponent<BaseWeapon>().SetParent(playerBehavior);
                    player.GetComponent<BaseWeapon>().SetFireRate(.45f);

                    player.AddComponent<TrailblazerUltimateAbility>();
                    player.GetComponent<TrailblazerUltimateAbility>().SetParent(playerBehavior);
                    SetupUltBars(playerOne, ship);
                }
            }
        }
    }

    private void SetupUltBars(bool forPlayerOne, string shipName)
    {
        if(forPlayerOne)
        {
            if(shipName == "Lancer")
            {
                player1LancerUlt.SetActive(true);
                player1VanguardUlt.SetActive(false);
                player1TrailblazerUlt.SetActive(false);
            }
            else if(shipName == "Vanguard")
            {
                player1LancerUlt.SetActive(false);
                player1VanguardUlt.SetActive(true);
                player1TrailblazerUlt.SetActive(false);
            }
            else
            {
                player1LancerUlt.SetActive(false);
                player1VanguardUlt.SetActive(false);
                player1TrailblazerUlt.SetActive(true);
            }
        }
        else
        {
            if (shipName == "Lancer")
            {
                player2LancerUlt.SetActive(true);
                player2VanguardUlt.SetActive(false);
                player2TrailblazerUlt.SetActive(false);
            }
            else if (shipName == "Vanguard")
            {
                player2LancerUlt.SetActive(false);
                player2VanguardUlt.SetActive(true);
                player2TrailblazerUlt.SetActive(false);
            }
            else
            {
                player2LancerUlt.SetActive(false);
                player2VanguardUlt.SetActive(false);
                player2TrailblazerUlt.SetActive(true);
            }
        }
    }


    // Methods for gameplay/level management.
    private IEnumerator StartGame()
    {
        if(SinglePlayer())
        {
            // Level 1 - Set number and name.
            SetLevelNumAndName(1, "A Walk in the Park (Except the Park is an Endless Void)");
            yield return new WaitForSeconds(LEVEL_INFO_FLASH_TIME);
            HideLevelNumAndName();

            // Level 1.
            levelAttach.AddComponent<LevelOne>();
            levelAttach.GetComponent<LevelOne>().SetSpawner(spawner);
            levelTime = levelAttach.GetComponent<LevelOne>().GetLevelTime();
            yield return new WaitForSeconds(levelTime);


            itemSelection.PresentItems(1);
            ClearEnemies();
            yield return new WaitUntil(() => itemSelection.DonePresenting());

            // Level 2 - Set number and name.
            SetLevelNumAndName(2, "Slightly More Enemies (This is Actually What We Have Written Down)");
            yield return new WaitForSeconds(LEVEL_INFO_FLASH_TIME);
            HideLevelNumAndName();

            // Level 2.
            levelAttach.AddComponent<LevelTwo>();
            levelAttach.GetComponent<LevelTwo>().SetSpawner(spawner);
            levelTime = levelAttach.GetComponent<LevelTwo>().GetLevelTime();
            yield return new WaitForSeconds(levelTime);


            itemSelection.PresentItems(2);
            ClearEnemies();
            yield return new WaitUntil(() => itemSelection.DonePresenting());

            // Level 3 - Set number and name.
            SetLevelNumAndName(3, "Revenge of the Ship (Wait, what?)");
            yield return new WaitForSeconds(LEVEL_INFO_FLASH_TIME);
            HideLevelNumAndName();

            // Level 3.
            levelAttach.AddComponent<LevelThree>();
            levelAttach.GetComponent<LevelThree>().SetSpawner(spawner);
            levelTime = levelAttach.GetComponent<LevelThree>().GetLevelTime();
            yield return new WaitForSeconds(levelTime);


            itemSelection.PresentItems(3);
            ClearEnemies();
            yield return new WaitUntil(() => itemSelection.DonePresenting());

            levelNumber = 4;

            // Level 4 - Set number and name.
            SetLevelNumAndName(4, "First Last Dance (I Swear There Were More Somewhere)");
            yield return new WaitForSeconds(LEVEL_INFO_FLASH_TIME);
            HideLevelNumAndName();

            // Level 4.
            levelAttach.AddComponent<LevelFour>();
            levelAttach.GetComponent<LevelFour>().SetSpawner(spawner);
            levelTime = levelAttach.GetComponent<LevelFour>().GetLevelTime();
            yield return new WaitForSeconds(levelTime);


            itemSelection.PresentItems(4);
            ClearEnemies();
            yield return new WaitUntil(() => itemSelection.DonePresenting());

            levelNumber++;
            // Endless Mode
            if (endless)
            {
                int EndlessName = 0;
                // By playing Endless Mode, the player is already a winner in my book. - Gary Yuen
                winLoss = true;
                while (player1.IsAlive())
                {
                    string lvlname = levelNames[EndlessName];
                    EndlessName++;
                    if (EndlessName == levelNames.Length)
                    {
                        EndlessName = 0;
                    }
                    // Level Endless - Set number and name.
                    SetLevelNumAndName(levelNumber, lvlname);
                    yield return new WaitForSeconds(LEVEL_INFO_FLASH_TIME);
                    HideLevelNumAndName();
                    levelNumber++;
                    // Level ENDLESS.
                    levelAttach.AddComponent<LevelENDLESS>();
                    levelAttach.GetComponent<LevelENDLESS>().SetSpawner(spawner);
                    levelAttach.GetComponent<LevelENDLESS>().SetTimeBetweenSpawns(TimeBetweenSpawns);
                    if (TimeBetweenSpawns > 1)
                    {
                        TimeBetweenSpawns -= .1f;
                        Debug.Log(TimeBetweenSpawns);
                    }
                    levelTime = levelAttach.GetComponent<LevelENDLESS>().GetLevelTime();
                    yield return new WaitForSeconds(levelTime);
                }
            }
            else
            {
                // Level 5 - Set number and name.
                SetLevelNumAndName(5, "Galaxy Buster");
                yield return new WaitForSeconds(LEVEL_INFO_FLASH_TIME);
                HideLevelNumAndName();

                // Level 5 (Boss fight).
                levelAttach.AddComponent<LevelFive>();
                levelAttach.GetComponent<LevelFive>().SetSpawner(spawner);
                yield return new WaitForSeconds(LEVEL_INFO_FLASH_TIME);
                while (bossAlive) yield return null;
                yield return new WaitForSeconds(LEVEL_INFO_FLASH_TIME);

                // Victory!
                if (player1.IsAlive())
                {
                    winLoss = true;
                    showBothScores = singlePlayer;
                    player1Score = GetComponent<ScoreManager>().GetPlayer1Score();
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                }
            }
        }
        // 2 Player Mode
        else 
        {
            // Level 1 - Set number and name.
            SetLevelNumAndName(1, "A Walk in the Park (Now With Extra Friendship!)");
            yield return new WaitForSeconds(LEVEL_INFO_FLASH_TIME);
            HideLevelNumAndName();

            // Level 1.
            levelAttach.AddComponent<LevelOne2P>();
            levelAttach.GetComponent<LevelOne2P>().SetSpawner(spawner);
            levelTime = levelAttach.GetComponent<LevelOne2P>().GetLevelTime();
            yield return new WaitForSeconds(levelTime);

            itemSelection.PresentItems(1);
            ClearEnemies();
            yield return new WaitUntil(() => itemSelection.DonePresenting());

            // Level 2 - Set number and name.
            SetLevelNumAndName(2, "Slightly More Enemies (Great, Now There Are Two Of Them.)");
            yield return new WaitForSeconds(LEVEL_INFO_FLASH_TIME);
            HideLevelNumAndName();

            // Level 2.
            levelAttach.AddComponent<LevelTwo2P>();
            levelAttach.GetComponent<LevelTwo2P>().SetSpawner(spawner);
            levelTime = levelAttach.GetComponent<LevelTwo2P>().GetLevelTime();
            yield return new WaitForSeconds(levelTime);


            itemSelection.PresentItems(2);
            ClearEnemies();
            yield return new WaitUntil(() => itemSelection.DonePresenting());

            // Level 3 - Set number and name.
            SetLevelNumAndName(3, "Revenge of the Ship (Or Would It Be Ships?)");
            yield return new WaitForSeconds(LEVEL_INFO_FLASH_TIME);
            HideLevelNumAndName();

            // Level 3.
            levelAttach.AddComponent<LevelThree2P>();
            levelAttach.GetComponent<LevelThree2P>().SetSpawner(spawner);
            levelTime = levelAttach.GetComponent<LevelThree2P>().GetLevelTime();
            yield return new WaitForSeconds(levelTime);


            itemSelection.PresentItems(3);
            ClearEnemies();
            yield return new WaitUntil(() => itemSelection.DonePresenting());

            // Level 4 - Set number and name.
            SetLevelNumAndName(4, "Last First Dance (Technically.)");
            yield return new WaitForSeconds(LEVEL_INFO_FLASH_TIME);
            HideLevelNumAndName();

            levelNumber = 4;

            // Level 4.
            levelAttach.AddComponent<LevelFour2P>();
            levelAttach.GetComponent<LevelFour2P>().SetSpawner(spawner);
            levelTime = levelAttach.GetComponent<LevelFour2P>().GetLevelTime();
            yield return new WaitForSeconds(levelTime);


            itemSelection.PresentItems(4);
            ClearEnemies();
            yield return new WaitUntil(() => itemSelection.DonePresenting());

            levelNumber++;

            // Endless Mode
            if (endless)
            {
                int EndlessName = 0;
                // By playing Endless Mode, the player is already a winner in my book. - Gary Yuen
                winLoss = true;
                while (player1.IsAlive() || player2.IsAlive())
                {
                    string lvlname = levelNames[EndlessName];
                    EndlessName++;
                    if (EndlessName == levelNames.Length)
                    {
                        EndlessName = 0;
                    }
                    // Level Endless - Set number and name.
                    SetLevelNumAndName(levelNumber, lvlname);
                    yield return new WaitForSeconds(LEVEL_INFO_FLASH_TIME);
                    HideLevelNumAndName();
                    levelNumber++;
                    // Level ENDLESS.
                    levelAttach.AddComponent<LevelENDLESS>();
                    levelAttach.GetComponent<LevelENDLESS>().SetSpawner(spawner);
                    levelAttach.GetComponent<LevelENDLESS>().SetTimeBetweenSpawns(TimeBetweenSpawns);
                    if (TimeBetweenSpawns > 1)
                    {
                        TimeBetweenSpawns -= .1f;
                    }
                    levelTime = levelAttach.GetComponent<LevelENDLESS>().GetLevelTime();
                    yield return new WaitForSeconds(levelTime);

                }
            }
            else
            {
                // Level 5 - Set number and name.
                SetLevelNumAndName(5, "Galaxy Busters");
                yield return new WaitForSeconds(LEVEL_INFO_FLASH_TIME);
                HideLevelNumAndName();

                // Level 5 (Boss fight).
                levelAttach.AddComponent<LevelFive>();
                levelAttach.GetComponent<LevelFive>().SetSpawner(spawner);
                yield return new WaitForSeconds(LEVEL_INFO_FLASH_TIME);
                while (bossAlive) yield return null;
                yield return new WaitForSeconds(LEVEL_INFO_FLASH_TIME);

                // Victory!
                if (player1.IsAlive() || (!singlePlayer && player2.IsAlive()))
                {
                    winLoss = true;
                    showBothScores = singlePlayer;
                    player1Score = GetComponent<ScoreManager>().GetPlayer1Score();
                    player2Score = GetComponent<ScoreManager>().GetPlayer2Score();
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                }
            }
        }
        
        
        
    }

    private void SetLevelNumAndName(int num, string name)
    {
        levelNum.text = "Level " + num.ToString();
        levelNum.enabled = true;
        levelName.text = name;
        levelName.enabled = true;
    }

    private void HideLevelNumAndName()
    {
        levelNum.enabled = false;
        levelName.enabled = false;
    }

    private void DetectCondition()
    {
        if (!player1.IsAlive() || (!singlePlayer && !player2.IsAlive()))
        {
            Debug.Log("PLAYER DIED");
            winLoss = false;
            showBothScores = singlePlayer;
            player1Score = GetComponent<ScoreManager>().GetPlayer1Score();
            player2Score = GetComponent<ScoreManager>().GetPlayer2Score();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        if(GameObject.FindWithTag("Boss"))
        {
            bossAlive = true;
        } else bossAlive = false;
    }

    // Meant to be called at the end of every level.
    // Destroys all enemies and enemy bullets currently onscreen.
    private void ClearEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] enemyBullets = GameObject.FindGameObjectsWithTag("EnemyProjectile");
        foreach(GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
        foreach(GameObject bullet in enemyBullets)
        {
            Destroy(bullet);
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1;
    }
}