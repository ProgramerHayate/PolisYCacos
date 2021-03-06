using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    public int clickedCop = 0;
    public int[,] matriu = new int[Constants.NumTiles, Constants.NumTiles];
    void Start()
    {        
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }
        
    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;            

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;                
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();                         
            }
        }
                
        cops[0].GetComponent<CopMove>().currentTile=Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile=Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile=Constants.InitialRobber;           
    }

    public void InitAdjacencyLists()
    {
        //Matriz de adyacencia
        matriu = new int[Constants.NumTiles, Constants.NumTiles];

        //Inicializar matriz a 0's

        for (int i = 0; i < Constants.NumTiles-1; i++)
        {
            for (int j = 0; j <Constants.NumTiles-1; j++)
            {
                matriu[i, j] = 0;
            }
        }


        //Para cada posición, rellenar con 1's las casillas adyacentes (arriba, abajo, izquierda y derecha)

        for (int i = 0; i < Constants.NumTiles-1; i++)
        {
            for (int j = 0; j < Constants.NumTiles-1; j++)
            {

                if (System.Math.Abs(i - j) == 1 || System.Math.Abs(i - j) == 8)
                {
                        matriu[i, j] = 1;
                }
                if (i%8 == 0  && i-j == 1)
                {
                    matriu[i, j] = 0;
                }
                if ((i+1)%8 == 0 && i-j == -1)
                {
                    matriu[i, j] = 0;
                }
            }
        }

        //Rellenar la lista "adjacency" de cada casilla con los índices de sus casillas adyacentes

        for (int i = 0; i < Constants.NumTiles - 1; i++)
        {
            for (int j = 0; j < Constants.NumTiles - 1; j++)
            {
                if (matriu[i,j] == 1 )
                {
                    tiles[i].adjacency.Add(j);
                }
            }
        }

    }

    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {        
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:                
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;                
                break;            
        }
    }

    public void ClickOnTile(int t)
    {                     
        clickedTile = t;

        switch (state)
        {            
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {                  
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile=tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;   
                    
                    state = Constants.TileSelected;
                }                
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {            
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:                
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }

    public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        /*Cambia el código de abajo para hacer lo siguiente
        - Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco
        - Movemos al caco a esa casilla
        - Actualizamos la variable currentTile del caco a la nueva casilla
        */


        int a = Random.Range(0, tiles[robber.GetComponent<RobberMove>().currentTile].adjacency.Count-1);

        robber.GetComponent<RobberMove>().MoveToTile(tiles[tiles[robber.GetComponent<RobberMove>().currentTile].adjacency[a]]);

        robber.GetComponent<RobberMove>().currentTile = tiles[tiles[robber.GetComponent<RobberMove>().currentTile].adjacency[a]].numTile;
        /*
        int max = 0;
        int pos = 0;
        
        for (int i = 0;i < tiles[robber.GetComponent<RobberMove>().currentTile].adjacency.Count; i++)
        {
            int prox1 = System.Math.Abs(tiles[robber.GetComponent<RobberMove>().currentTile].adjacency[i] - cops[0].GetComponent<CopMove>().currentTile);
            int prox2 = System.Math.Abs(tiles[robber.GetComponent<RobberMove>().currentTile].adjacency[i] - cops[1].GetComponent<CopMove>().currentTile);
            int prox = prox1 + prox2;

            
            if (prox > max)
            {
                max = prox;
                pos = i;
            }
        }
        robber.GetComponent<RobberMove>().MoveToTile(tiles[tiles[robber.GetComponent<RobberMove>().currentTile].adjacency[pos]]);
        robber.GetComponent<RobberMove>().currentTile = tiles[tiles[robber.GetComponent<RobberMove>().currentTile].adjacency[pos]].numTile;
        */
    }

    public void EndGame(bool end)
    {
        if(end)
            finalMessage.text = "You Win!";
        else
            finalMessage.text = "You Lose!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);
                
        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;
         
    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public void FindSelectableTiles(bool cop)
    {
                 
        int indexcurrentTile;        

        if (cop==true)
            indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile;
        else
            indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;

        //La ponemos rosa porque acabamos de hacer un reset
        tiles[indexcurrentTile].current = true;

        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        //Implementar BFS. Los nodos seleccionables los ponemos como selectable=true
        //Tendrás que cambiar este código por el BFS
        for(int i = 0; i < Constants.NumTiles -1; i++)
        {
            
           
            
            // comprobaciones para saber las casillas adyacentes
            if (matriu[cops[clickedCop].GetComponent<CopMove>().currentTile, i] == 1)
            {
                tiles[i].selectable = true;
            }
            if (cops[clickedCop].GetComponent<CopMove>().currentTile - 1 >= 0 && cops[clickedCop].GetComponent<CopMove>().currentTile - 1 <= 64 &&  matriu[cops[clickedCop].GetComponent<CopMove>().currentTile - 1, i] == 1)
            {
                tiles[i].selectable = true;

                // comprobaciones para que no se pisen
                if (cops[clickedCop].Equals(cops[0]) && cops[clickedCop].GetComponent<CopMove>().currentTile -1 == cops[1].GetComponent<CopMove>().currentTile)
                {
                    tiles[i].selectable = false;

                }
                if (cops[clickedCop].Equals(cops[1]) && cops[clickedCop].GetComponent<CopMove>().currentTile - 1 == cops[0].GetComponent<CopMove>().currentTile)
                {
                    tiles[i].selectable = false;
                }
            }
            if (cops[clickedCop].GetComponent<CopMove>().currentTile + 1 >= 0 && cops[clickedCop].GetComponent<CopMove>().currentTile + 1 <= 64 && matriu[cops[clickedCop].GetComponent<CopMove>().currentTile + 1, i] == 1)
            {
                tiles[i].selectable = true;

                // comprobaciones para que no se pisen
                if (cops[clickedCop].Equals(cops[0]) && cops[clickedCop].GetComponent<CopMove>().currentTile + 1 == cops[1].GetComponent<CopMove>().currentTile)
                {
                    tiles[i].selectable = false;

                }

                if (cops[clickedCop].Equals(cops[1]) && cops[clickedCop].GetComponent<CopMove>().currentTile + 1 == cops[0].GetComponent<CopMove>().currentTile)
                {
                    tiles[i].selectable = false;
                }
            }
            if (cops[clickedCop].GetComponent<CopMove>().currentTile - 8 >= 0 && cops[clickedCop].GetComponent<CopMove>().currentTile - 8 <= 64 && matriu[cops[clickedCop].GetComponent<CopMove>().currentTile - 8, i] == 1)
            {
                tiles[i].selectable = true;

                // comprobaciones para que no se pisen
                if (cops[clickedCop].Equals(cops[0]) && cops[clickedCop].GetComponent<CopMove>().currentTile - 8 == cops[1].GetComponent<CopMove>().currentTile && System.Math.Abs(cops[0].GetComponent<CopMove>().currentTile - i) > 10)
                {
                    tiles[i].selectable = false;

                }

                if (cops[clickedCop].Equals(cops[1]) && cops[clickedCop].GetComponent<CopMove>().currentTile - 8 == cops[0].GetComponent<CopMove>().currentTile && System.Math.Abs(cops[1].GetComponent<CopMove>().currentTile - i) > 10)
                {
                    tiles[i].selectable = false;
                }
            }
            if (cops[clickedCop].GetComponent<CopMove>().currentTile + 8 >= 0 && cops[clickedCop].GetComponent<CopMove>().currentTile + 8 <= 64 && matriu[cops[clickedCop].GetComponent<CopMove>().currentTile + 8, i] == 1)
            {
                tiles[i].selectable = true;

                // comprobaciones para que no se pisen
                if (cops[clickedCop].Equals(cops[0]) && cops[clickedCop].GetComponent<CopMove>().currentTile + 8 == cops[1].GetComponent<CopMove>().currentTile && System.Math.Abs(cops[0].GetComponent<CopMove>().currentTile - i) > 10)
                {
                    tiles[i].selectable = false;

                }

                if (cops[clickedCop].Equals(cops[1]) && cops[clickedCop].GetComponent<CopMove>().currentTile + 8 == cops[0].GetComponent<CopMove>().currentTile && System.Math.Abs(cops[1].GetComponent<CopMove>().currentTile - i) > 10)
                {
                    tiles[i].selectable = false;
                }
            }

            // comprobaciones para no cambiar de lado del tablero
            if (cops[clickedCop].GetComponent<CopMove>().currentTile % 8 == 0 && cops[clickedCop].GetComponent<CopMove>().currentTile - i == -7)
            {
                tiles[i].selectable = false;
            }
            if (cops[clickedCop].GetComponent<CopMove>().currentTile % 8 == 0 && cops[clickedCop].GetComponent<CopMove>().currentTile - i == 2)
            {
                tiles[i].selectable = false;
            }
            if (cops[clickedCop].GetComponent<CopMove>().currentTile % 8 == 0 && cops[clickedCop].GetComponent<CopMove>().currentTile - i == 9)
            {
                tiles[i].selectable = false;
            }
            if ((cops[clickedCop].GetComponent<CopMove>().currentTile + 1) % 8 == 0 && cops[clickedCop].GetComponent<CopMove>().currentTile - i == 7)
            {
                tiles[i].selectable = false;
            }
            if ((cops[clickedCop].GetComponent<CopMove>().currentTile + 1) % 8 == 0 && cops[clickedCop].GetComponent<CopMove>().currentTile - i == -2)
            {
                tiles[i].selectable = false;
            }
            if ((cops[clickedCop].GetComponent<CopMove>().currentTile + 1) % 8 == 0 && cops[clickedCop].GetComponent<CopMove>().currentTile - i == -9)
            {
                tiles[i].selectable = false;
            }

            // comprobaciones para que no se pisen
            if (cops[clickedCop].Equals(cops[0]) && cops[1].GetComponent<CopMove>().currentTile == i)
            {
                tiles[i].selectable = false;
                
            }
           
            if (cops[clickedCop].Equals(cops[1]) && cops[0].GetComponent<CopMove>().currentTile == i)
            {
                tiles[i].selectable = false;
            }
            //no moverse a la misma casilla
            if (cops[clickedCop].GetComponent<CopMove>().currentTile.Equals(i))
            {
                tiles[i].selectable = false;
            }
        }


    }
    
   
    

    

   

       
}
