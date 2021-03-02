﻿using System;
using System.Collections.Generic;
using UnityEngine;
using DiceyDungeonsAR.GameObjects.Players;
using DiceyDungeonsAR.GameObjects;
using DiceyDungeonsAR.Enemies;
using System.Collections;
using DiceyDungeonsAR.Battle;

namespace DiceyDungeonsAR.MyLevelGraph
{
    public class LevelGraph : MonoBehaviour
    {
        public float MAX_LEAF_SIZE = 2f, MIN_LEAF_SIZE = 1f;
        public Vector2 mapCorner, mapSize; // левый "нижний" угол карты и её размер
        readonly List<Leaf> leaves = new List<Leaf>(); // список всех листьев

        [SerializeField] Apple applePrefab;
        [SerializeField] Chest chestPrefab;
        [SerializeField] Shop shopPrefab;
        [SerializeField] Exit exitPrefab;
        [SerializeField] GameObject fieldPrefab;
        [SerializeField] Edge edgePrefab;
        [SerializeField] List<EnemyItem> enemies1Level, enemies2Level, enemies3Level, bosses;

        static public LevelGraph levelGraph;

        [NonSerialized] public readonly List<Field> fields = new List<Field>();
        [NonSerialized] public Player player;
        [NonSerialized] public BattleController battle;

        IEnumerator Start()
        {
            levelGraph = this;
            battle = GetComponent<BattleController>();

            GenerateLevel();

            player = FindObjectOfType<Player>();
            if (player == null)
            {
                Debug.LogWarning("Player wasn't found");
                yield break;
            }

            yield return null; // подождать инициализацию полей
            player.Initialize();
        }

        public void GenerateLevel()
        {
            Leaf.MIN_SIZE = MIN_LEAF_SIZE;
            var root = new Leaf(mapCorner, mapSize); // "корень" для всех остальных листьев
            leaves.Add(root);

            // снова и снова проходим по каждому листу, пока больше не останется листьев, которые можно разрезать
            List<Leaf> toBeChecked = new List<Leaf>(leaves); // очередь для проверки новых листев
            var doSplit = true;
            while (doSplit)
            {
                doSplit = false; // сбрасываем флаг
                var copy = new List<Leaf>(toBeChecked); // создаём копию, так как нельзя менять список во время его цикла
                toBeChecked.Clear(); // очищаем очередь
                foreach (var l in copy)
                {
                    if (l.leftChild == null && l.rightChild == null) // если лист ещё не разрезан...
                        if (l.width > MAX_LEAF_SIZE || l.length > MAX_LEAF_SIZE || UnityEngine.Random.value > 0.25)
                        // если этот лист слишком велик, или есть вероятность 75%...
                        {
                            if (l.Split()) // пытаемя разрезаеть лист
                            {
                                // если получилось, добавляем обе части в список, чтобы в дальнейшем можно было в цикле проверить и их
                                toBeChecked.Add(l.leftChild);
                                toBeChecked.Add(l.rightChild);
                                leaves.Add(l.leftChild); // добавляем в общий список листев
                                leaves.Add(l.rightChild);
                                doSplit = true; // включаем следующую итерацию для проверки полученных частей
                            }
                        }
                }
            }
            //print(leaves.Count);

            //.PlaceItem(Instantiate(chestPrefab))
            //AddField(-1.60f, -1.00f);
            //AddField(-0.56f, -1.24f).PlaceItem(Instantiate(enemies1Level[0]));
            //AddField(-0.58f, -0.37f);
            //AddField(-0.82f, 0.63f);
            //AddField(-0.76f, 1.54f).PlaceItem(Instantiate(enemies2Level[0]));
            //AddField(0.10f, 1.64f).PlaceItem(Instantiate(applePrefab));
            //AddField(0.18f, 0.55f).PlaceItem(Instantiate(enemies3Level[0]));
            //AddField(1.00f, 1.00f).PlaceItem(Instantiate(bosses[0]));
            //AddField(1.56f, 0.13f).PlaceItem(Instantiate(exitPrefab));

            //AddField(0.29f, -1.57f).PlaceItem(Instantiate(chestPrefab));
            //AddField(-1.52f, -0.04f).PlaceItem(Instantiate(applePrefab));
            //AddField(-1.46f, 0.99f).PlaceItem(Instantiate(chestPrefab));
            //AddField(0.33f, -0.43f).PlaceItem(Instantiate(shopPrefab));

            for (int i = 0; i < fields.Count - 5; i++)
            {
                AddEdge(i, i + 1);
            }

            AddEdge(1, 9);
            AddEdge(2, 10);
            AddEdge(4, 11);
            AddEdge(6, 12);
        }

        public Field AddField(float x, float z)
        {
            var field = Instantiate(fieldPrefab, transform).GetComponentInChildren<Field>();
            field.Initialize(this, x, 0, z);
            return field;
        }
 
        public Field FindField(string vertexName)
        {
            foreach (var f in fields)
            {
                if (f.name.Equals(vertexName))
                {
                    return f;
                }
            }
 
            return null;
        }
 
        public void AddEdge(string firstName, string secondName, int weight = 0)
        {
            var f1 = FindField(firstName);
            var f2 = FindField(secondName);
            if (f2 != null && f1 != null)
            {
                AddEdge(f1, f2, weight);
            }
        }
        
        public void AddEdge(Field first, Field second, int weight = 0)
        {
            if (fields.Contains(first) && fields.Contains(second))
            {
                Edge edge = Instantiate(edgePrefab);
                edge.Initialize(this, first, second, weight);
                first.AddEdge(edge);
                second.AddEdge(edge);
            }
        }

        public void AddEdge(int firstIndex, int secondIndex, int weight = 0)
        {
            if (Mathf.Max(firstIndex, secondIndex) < fields.Count && Mathf.Min(firstIndex, secondIndex) >= 0)
                AddEdge(fields[firstIndex], fields[secondIndex], weight);
        }

        public IEnumerator StartBattle(Enemy enemy)
        {
            AppearingAnim msg = AppearingAnim.CreateMsg("StartBattleMsg", new Vector2(0.21f, 0.37f), new Vector2(0.78f, 0.68f), "Битва начинается");
            msg.color = Color.red;
            msg.yOffset = 50;
            msg.period = 2;
            msg.Play();

            battle.SetUpOpponents(enemy);

            for (int i = 2; i < transform.childCount-1; i++)
                transform.GetChild(i).gameObject.SetActive(false);

            player.transform.GetChild(0).gameObject.SetActive(false);
            enemy.transform.GetChild(0).gameObject.SetActive(false);

            yield return new WaitForSeconds(2.0f);

            player.transform.GetChild(0).gameObject.SetActive(true);
            enemy.transform.GetChild(0).gameObject.SetActive(true);

            StartCoroutine(battle.StartBattle());
        }
    }
}
