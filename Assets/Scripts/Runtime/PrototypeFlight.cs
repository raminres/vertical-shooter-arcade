using System;
using DG.Tweening;
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
        [SerializeField] private PrototypeEffects effects;
        [SerializeField] private PrototypeSpecialWeapon special;

        private Vector3[] enemyDirections;
        private int[] enemyHealth;
        private int[] enemyGeneration;
        private Vector3[] enemyBaseScales;
        private float[] enemyHitTimers;
        public event Action EnemyDefeated;
        public event Action<bool> EnemyHit;
        private int group;
        private int spawned;
        private float timer;
        private bool dragging;
        private Vector2 previousTouch;
        private Quaternion visualRestRotation;
        private float baseFieldOfView;
        private float bankStart, bankTarget, bankValue, bankElapsed;
        private float cameraStart, cameraTarget, cameraElapsed;
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
            visualRestRotation = visual.localRotation;
            baseFieldOfView = flightCamera.fieldOfView;
            bankStart = bankTarget = bankValue = 0f;
            cameraStart = cameraTarget = baseFieldOfView;
            bankElapsed = cameraElapsed = 0f;
        }

        private void OnDisable()
        {
            if (flightCamera != null) flightCamera.fieldOfView = baseFieldOfView;
            if (visual != null) visual.localRotation = visualRestRotation;
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
            if (deltaTime <= 0f || combat.IsDefeated) return;
            combat.CaptureEnemies();
            special.CaptureEnemies();
            transform.position += Vector3.forward * (settings.scrollSpeed * deltaTime);
            foreach (var tile in groundSections)
            {
                tile.localPosition -= Vector3.forward * (settings.scrollSpeed * deltaTime);
                if (tile.localPosition.z < -groundLength)
                    tile.localPosition += Vector3.forward * (groundLength * groundSections.Length);
            }
            float touchBank = drag.x / (deltaTime * Mathf.Max(.01f, settings.touchBankFullSpeed));
            float bankInput = Mathf.Clamp(move.x + touchBank, -1f, 1f);
            float cameraInput = Mathf.Clamp(move.x + drag.x * 20f, -1f, 1f);
            ApplyMotionTweens(deltaTime, bankInput, cameraInput);
            var lower = PointOnPlane(new Vector2(0.5f, 0.13f), aircraft.position.y);
            var upper = PointOnPlane(new Vector2(0.5f, 0.84f), aircraft.position.y);
            Vector3 position = aircraft.position;
            position.z += move.y * settings.playerSpeed * deltaTime + drag.y * (upper.z - lower.z) * settings.touchSensitivity;
            float halfLength = Mathf.Min(settings.playerHalfLength, (upper.z - lower.z) * 0.5f);
            position.z = Mathf.Clamp(position.z, lower.z + halfLength, upper.z - halfLength);
            // The near edge is narrower when the camera is tilted, so constrain both wings there.
            float nearViewportY = flightCamera.WorldToViewportPoint(new Vector3(position.x, position.y, position.z - halfLength)).y;
            var left = PointOnPlane(new Vector2(0.06f, nearViewportY), position.y);
            var right = PointOnPlane(new Vector2(0.94f, nearViewportY), position.y);
            position.x += move.x * settings.playerSpeed * deltaTime + drag.x * (right.x - left.x) * settings.touchSensitivity;
            float halfWidth = Mathf.Min(settings.playerHalfWidth, (right.x - left.x) * 0.5f);
            position.x = Mathf.Clamp(position.x, left.x + halfWidth, right.x - halfWidth);
            aircraft.position = position;
            TickEnemies(deltaTime);
            combat.Simulate(deltaTime);
            special.Simulate(deltaTime);
        }

        private void ApplyMotionTweens(float deltaTime, float bankInput, float cameraInput)
        {
            float desiredBank = -bankInput * settings.bankAngle;
            if (!Mathf.Approximately(desiredBank, bankTarget))
            {
                bankStart = bankValue;
                bankTarget = desiredBank;
                bankElapsed = 0f;
            }
            bankElapsed += deltaTime;
            float bankProgress = Mathf.Clamp01(bankElapsed / Mathf.Max(.01f, settings.bankTweenDuration));
            bankValue = DOVirtual.EasedValue(bankStart, bankTarget, bankProgress, Ease.OutCubic);
            visual.localRotation = visualRestRotation * Quaternion.Euler(0f, 0f, bankValue);

            float desiredFov = baseFieldOfView + Mathf.Abs(cameraInput) * settings.cameraMovementZoom;
            if (!Mathf.Approximately(desiredFov, cameraTarget))
            {
                cameraStart = flightCamera.fieldOfView;
                cameraTarget = desiredFov;
                cameraElapsed = 0f;
            }
            cameraElapsed += deltaTime;
            float cameraProgress = Mathf.Clamp01(cameraElapsed / Mathf.Max(.01f, settings.cameraTweenDuration));
            flightCamera.fieldOfView = DOVirtual.EasedValue(cameraStart, cameraTarget, cameraProgress, Ease.OutCubic);
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
                enemyHitTimers[index] = Mathf.Max(.01f, settings.enemyHitTweenDuration);
                enemies[index].localScale = enemyBaseScales[index] * .8f;
                EnemyHit?.Invoke(false);
                return false;
            }
            effects.PlayEnemyExplosion(enemies[index].position);
            enemies[index].gameObject.SetActive(false);
            ActiveEnemies--;
            EnemyHit?.Invoke(true);
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
                    float progress = 1f - Mathf.Clamp01(enemyHitTimers[i] / Mathf.Max(.01f, settings.enemyHitTweenDuration));
                    float scale = DOVirtual.EasedValue(.8f, 1f, progress, Ease.OutBack);
                    enemies[i].localScale = enemyBaseScales[i] * scale;
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
            int count = Mathf.Max(1, settings.enemiesPerGroup);
            if (spawned < count && timer <= 0f)
            {
                for (int i = 0; i < enemies.Length; i++)
                {
                    if (enemies[i].gameObject.activeSelf) continue;
                    float x = group == 0 ? 0.32f + (spawned % 3) * 0.18f : (group == 1 ? 0.22f : 0.78f);
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
