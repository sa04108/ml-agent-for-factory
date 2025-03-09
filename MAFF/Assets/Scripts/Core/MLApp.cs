using System;
using System.Collections.Generic;
using UnityEngine;

namespace MlAgent
{
    public class MLApp : MonoBehaviour
    {
        private static Dictionary<Type, Service> services = new();

        [Header("Agent")]
        [SerializeField] private int agentNumber = 4;
        [SerializeField] private Transform agentParent;
        [SerializeField] private Agent agentPrefab;

        [Header("Path")]
        [SerializeField] private PathManager pathPrefab;
        [SerializeField] private PathAlgorithm pathAlgorithm;
        private int agentIndex = 0;

        private void Start()
        {
            CreatePath();

            for (int i = 0; i < agentNumber; i++)
            {
                var pos = GetService<PathManager>()
                    .GetRandomWayPoint();

                CreateAgent(pos);
            }
        }

        private void AddService<T>() where T : MonoBehaviour, Service
        {
            GameObject go = new GameObject(nameof(T));
            go.transform.SetParent(transform);
            var compo = go.AddComponent<T>();

            compo.OnInit();
            services.Add(typeof(T), compo);
        }

        public static T GetService<T>() where T : MonoBehaviour, Service
        {
            return services[typeof(T)] as T;
        }

        private void CreatePath()
        {
            var pm = Instantiate(pathPrefab, transform);
            pm.SetPathAlgorithm(pathAlgorithm);
            pm.OnInit();
            services.Add(typeof(PathManager), pm);
        }

        private void CreateAgent(Vector3 position)
        {
            var ag = Instantiate(agentPrefab, agentParent);
            ag.transform.position = position;
            ag.name = $"Agent {++agentIndex}";

            ag.OnCreate();
        }
    }
}