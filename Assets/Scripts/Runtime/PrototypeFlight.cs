using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Screen = UnityEngine.Device.Screen;

namespace VerticalShooter.Gameplay
{
    public sealed class PrototypeFlight : MonoBehaviour
    {
        [SerializeField] private FlightSettings settings;
        [SerializeField] private Camera flightCamera;
        [SerializeField] private Transform aircraft;
        [SerializeField] private Transform visual;
        [SerializeField] private Transform[] groundSections;
        [SerializeField] private Transform[] enemies;
        [SerializeField] private float groundLength = 30f;
        [SerializeField] private PrototypeCombat combat;

        private Vector3[] enemyDirections;
        private int[] enemyHealth;
        private int[] enemyGeneration;
        private Vector3[] enemyBaseScales;
        private float[] enemyHitTimers;
        public event Action EnemyDefeated;
        private int group;
        private int spawned;
        private float timer;
        private bool dragging;
        private Vector2 previousTouch;
        public int Capacity => enemies.Length;
        public Transform GetEnemy(int index) => enemies[index];
        public int GetEnemyHealth(int index) => enemyHealth[index];
        public int GetEnemyGeneration(int index) => enemyGeneration[index];
        public int ActiveEnemies { get; private set; }
        public Camera FlightCamera => flightCamera;
        public Transform Aircraft => aircraft;

        private void OnEnable()
        {
            enemyDirections = new Vector3[enemies.Length];
            enemyHealth = new int[enemies.Length];
            enemyGeneration = new int[enemies.Length];
            enemyBaseScales = new Vector3[enemies.Length];
            enemyHitTimers = new float[enemies.Length];
            for (int i = 0; i < enemies.Length; i++)
            {
                enemyBaseScales[i] = enemies[i].localScale;
                enemies[i].gameObject.SetActive(false);
            }
            ActiveEnemies = 0;
            group = 0;
            spawned = 0;
            timer = settings.groupInterval;
            dragging = false;
        }

        private void Update()
        {
            if (Time.timeScale == 0f) { dragging = false; return; }
            Vector2 move = Vector2.zero;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                move.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0);
                move.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1 : 0);
                move = Vector2.ClampMagnitude(move, 1f);
            }
            Vector2 drag = Vector2.zero;
            var touch = Touchscreen.current?.primaryTouch;
            if (touch != null)
            {
                Vector2 position = touch.position.ReadValue();
                if (touch.press.wasPressedThisFrame)
                {
                    dragging = Screen.height > 0 && position.y / Screen.height > 0.12f && position.y / Screen.height < 0.84f;
                    previousTouch = position;
                }
                if (dragging && touch.press.isPressed && Screen.width > 0 && Screen.height > 0)
                {
                    drag = new Vector2((position.x - previousTouch.x) / Screen.width, (position.y - previousTouch.y) / Screen.height);
                    previousTouch = position;
                }
                if (!touch.press.isPressed) dragging = false;
            }
            Simulate(Time.deltaTime, move, drag);
        }

        public void Simulate(float deltaTime, Vector2 move, Vector2 drag)
        {
            if (deltaTime <= 0f) return;
            combat.CaptureEnemies();
            transform.position += Vector3.forward * (settings.scrollSpeed * deltaTime);
            foreach (var tile in groundSections)
            {
                tile.localPosition -= Vector3.forward * (settings.scrollSpeed * deltaTime);
                if (tile.localPosition.z < -groundLength)
                    tile.localPosition += Vector3.forward * (groundLength * groundSections.Length);
            }
            var lower = PointOnPlane(new Vector2(0.06f, 0.13f), aircraft.position.y);
            var upper = PointOnPlane(new Vector2(0.94f, 0.84f), aircraft.position.y);
            Vector3 position = aircraft.position;
            position.x += move.x * settings.playerSpeed * deltaTime + drag.x * (upper.x - lower.x) * settings.touchSensitivity;
            position.z += move.y * settings.playerSpeed * deltaTime + drag.y * (upper.z - lower.z) * settings.touchSensitivity;
            float halfWidth = Mathf.Min(settings.playerHalfWidth, (upper.x - lower.x) * 0.5f);
            float halfLength = Mathf.Min(settings.playerHalfLength, (upper.z - lower.z) * 0.5f);
            position.x = Mathf.Clamp(position.x, lower.x + halfWidth, upper.x - halfWidth);
            position.z = Mathf.Clamp(position.z, lower.z + halfLength, upper.z - halfLength);
            aircraft.position = position;
            float bankInput = Mathf.Clamp(move.x + drag.x * 20f, -1f, 1f);
            visual.localRotation = Quaternion.Slerp(visual.localRotation, Quaternion.Euler(0f, 0f, -bankInput * settings.bankAngle), 1f - Mathf.Exp(-10f * deltaTime));
            TickEnemies(deltaTime);
            combat.Simulate(deltaTime);
        }

        private Vector3 PointOnPlane(Vector2 viewport, float height)
        {
            Ray ray = flightCamera.ViewportPointToRay(new Vector3(viewport.x, viewport.y, 0f));
            float distance = (height - ray.origin.y) / ray.direction.y;
            return ray.GetPoint(distance);
        }

        public bool DamageEnemy(int index, int damage)
        {
            if (index < 0 || index >= enemies.Length || damage <= 0 || !enemies[index].gameObject.activeSelf)
                return false;
            enemyHealth[index] = Mathf.Max(0, enemyHealth[index] - damage);
            if (enemyHealth[index] > 0)
            {
                enemyHitTimers[index] = .12f;
                enemies[index].localScale = enemyBaseScales[index] * .8f;
                return false;
            }
            enemies[index].gameObject.SetActive(false);
            ActiveEnemies--;
            EnemyDefeated?.Invoke();
            return true;
        }

        private void TickEnemies(float deltaTime)
        {
            float exitZ = PointOnPlane(new Vector2(0.5f, -0.15f), aircraft.position.y).z;
            for (int i = 0; i < enemies.Length; i++)
            {
                if (!enemies[i].gameObject.activeSelf) continue;
                if (enemyHitTimers[i] > 0f)
                {
                    enemyHitTimers[i] -= deltaTime;
                    if (enemyHitTimers[i] <= 0f) enemies[i].localScale = enemyBaseScales[i];
                }
                enemies[i].position += enemyDirections[i] * (settings.enemySpeed * deltaTime);
                if (enemies[i].position.z < exitZ)
                {
                    enemies[i].gameObject.SetActive(false);
                    ActiveEnemies--;
                }
            }
            timer -= deltaTime;
            int count = group == 0 ? 2 : 3;
            if (spawned < count && timer <= 0f)
            {
                for (int i = 0; i < enemies.Length; i++)
                {
                    if (enemies[i].gameObject.activeSelf) continue;
                    float x = group == 0 ? 0.5f : (group == 1 ? 0.22f : 0.78f);
                    enemies[i].position = PointOnPlane(new Vector2(x, 1.12f), aircraft.position.y);
                    enemyDirections[i] = group == 0 ? Vector3.back : new Vector3(group == 1 ? 0.3f : -0.3f, 0f, -1f).normalized;
                    enemyHitTimers[i] = 0f;
                    enemies[i].localScale = enemyBaseScales[i];
                    enemyHealth[i] = Mathf.Max(1, settings.lightFighterHitPoints);
                    enemyGeneration[i]++;
                    enemies[i].gameObject.SetActive(true);
                    ActiveEnemies++;
                    spawned++;
                    timer = settings.enemySpawnInterval;
                    break;
                }
            }
            if (spawned == count && ActiveEnemies == 0)
            {
                group = (group + 1) % 3;
                spawned = 0;
                timer = settings.groupInterval;
            }
        }
    }
}
