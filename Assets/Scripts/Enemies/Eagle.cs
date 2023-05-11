using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies
{
    public class Eagle : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 2f;
        //[SerializeField] int attackDamage = 10;
        [SerializeField] private Transform player;

        private Animator animator;
        private new Rigidbody rigidbody;
        private BoxCollider boxCollider;

        private int currentWaypointIndex;
        private float patrolSpeed;
        private float attackTimer;
        private bool isAttacking;
        private bool isDead;
        private Vector3 cubeCenter,
            cubeSize,
            lookAtPlayerPos;

        private enum EnemyState
        {
            Patrol,
            Attack
        }
    
        private EnemyState currentState = EnemyState.Patrol;

        /* Animation */
        private static readonly int AAttacking = Animator.StringToHash("isAttacking");
        private static readonly int AFlying = Animator.StringToHash("isFlying");
        private static readonly int ADie = Animator.StringToHash("Die");
    
    
        /* Repeatedly used variables */
        private Transform trans;
        private Vector3 transPos;
        private Vector3 transScale;
        private Vector3 playerPos;
    
    
        /* Factory Variables */
        private bool defaultSetActive;
        private bool defaultRigidbodyGravity;
        private bool defaultEnabledCollider;
        private bool defaultTriggerCollider;
        private Vector3 defaultColliderCenter;
        private Vector3 defaultColliderSize;
        private Vector3 defaultPosition;
        private Quaternion defaultRotation;

        #region Unity Functions

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            if (isDead) return;
            Tick();
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Check for the Projectile Layer from the "P_LPSP_PROJ_Bullet_01" prefab (bullet)
            if (collision.gameObject.layer == 15)
            {
                Die();
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(cubeCenter, cubeSize);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transPos + new Vector3(0, 0, -attackRange), transPos + new Vector3(0, 0, attackRange));
            //Gizmos.DrawWireSphere(transPos, attackRange);
        }

        #endregion

    
    
        #region Userdefined Functions

        private void Patrol()
        {
            //var transformPos = transform.position;
            Vector3 nextWaypointPos = waypoints[currentWaypointIndex].position;
            Vector3 direction = (nextWaypointPos - transPos).normalized;
            var distance = Vector3.Distance(transPos, nextWaypointPos);
        
            // Move to next point
            transform.position = Vector3.MoveTowards(transPos, nextWaypointPos, 
                patrolSpeed * Time.deltaTime);
            
            // Rotate towards the target direction
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        
        
            if (distance < 0.1f)
            {
                SetNextWaypoint();
            }
        
            if (IsPlayerInRange())
            {
                currentState = EnemyState.Attack;
            }
        }

        private void Attack()
        {
            if (IsPlayerInRange())
            {
                var direction = (player.position - transform.position).normalized;
                var distance = Vector3.Distance(transPos, playerPos);
                
                // If player out of range => MoveTowards him
                if (distance > attackRange)
                {
                    transform.position = Vector3.MoveTowards(transform.position, player.position, 
                        patrolSpeed * Time.deltaTime);
                }
                else
                {
                    if (!isAttacking)
                    {
                        isAttacking = true;
                        animator.SetBool(AAttacking, true);
                        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                        transform.LookAt(lookAtPlayerPos);
                        attackTimer = attackCooldown;
                    }
        
                    if (attackTimer > 0)
                    {
                        attackTimer -= Time.deltaTime;
                    }
                    else
                    {
                        isAttacking = false;
                        animator.SetBool(AAttacking, false);
                        currentState = EnemyState.Patrol;
                    }
                }
            }
            else
            {
                if (isAttacking)
                {
                    isAttacking = false;
                    animator.SetBool(AAttacking, false);
                    currentState = EnemyState.Patrol;
                }
                else
                {
                    currentState = EnemyState.Patrol;
                }
            }
        }

        private bool IsPlayerInRange()
        {
            Collider[] colliders = Physics.OverlapBox(cubeCenter, cubeSize);
            foreach (Collider colliderr in colliders)
            {
                if (colliderr.gameObject.tag.Equals("Player"))
                {
                    return true;
                }
            }
            return false;
        }
        
        private void Init()
        {
            animator = GetComponent<Animator>();
            rigidbody = GetComponent<Rigidbody>();
            boxCollider = GetComponent<BoxCollider>();
            
            patrolSpeed = Random.Range(5, 15);

            SetFactoryVariables();
            UpdateVariables();
            SetNextWaypoint();
        }

        private void SetFactoryVariables()
        {
            defaultSetActive = gameObject.activeSelf;
            defaultRigidbodyGravity = rigidbody.useGravity;
            defaultEnabledCollider = boxCollider.enabled;
            defaultTriggerCollider = boxCollider.isTrigger;
            defaultColliderCenter = boxCollider.center;
            defaultColliderSize = boxCollider.size;
            defaultPosition = transform.position;
            defaultRotation = transform.rotation;
        }

        private void Tick()
        {
            UpdateVariables();
            switch (currentState)
            {
                case EnemyState.Patrol:
                    Patrol();
                    break;

                case EnemyState.Attack:
                    Attack();
                    break;
            }
            UpdateAnimation();
        }
        
        private void UpdateVariables()
        {
            UpdateTransVariables();
            
            cubeCenter = new Vector3(transPos.x, transPos.y - 3, transPos.z);
            cubeSize = new Vector3(transScale.x, 10, transScale.z + 10);
            lookAtPlayerPos = player.position + (player.up * 5);
        }
        
        private void UpdateTransVariables()
        {
            trans = transform;
            transPos = trans.position;
            transScale = trans.localScale;
            playerPos = player.position;
        }
        
        private void SetNextWaypoint()
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
        
        private void UpdateAnimation()
        {
            if (currentState == EnemyState.Patrol)
            {
                animator.SetBool(AFlying, true);
                animator.SetBool(AAttacking, false);
            }
            else if (currentState == EnemyState.Attack)
            {
                animator.SetBool(AFlying, false);
                animator.SetBool(AAttacking, true);
            }
        }
        
        private void Die()
        {
            if (isDead) return;

            //var proj = Vector3.ProjectOnPlane(transform.up, Vector3.down);
            //trans.position = new Vector3(transPos.x, proj.y + transPos.y, transPos.z);
            isDead = true;
            animator.SetBool(AFlying, false);
            animator.SetBool(AAttacking, false);
            //trans.rotation = quaternion.identity;
            rigidbody.useGravity = true;
            boxCollider.center = new Vector3(-0.005f, 0.04f, -0.61f);
            boxCollider.size = new Vector3(0.01f, 0.01f, 0.01f);
            animator.SetTrigger(ADie);
            Invoke(nameof(DestroyEnemy), 5);
            Invoke(nameof(Revive), Random.Range(7, 10));
        }
        
        private void DestroyEnemy()
        {
            gameObject.SetActive(false);
            boxCollider.enabled = false;
            boxCollider.isTrigger = true;
        }

        private void Revive()
        {
            isDead = false;
            ResetFactorySettings();
        }

        void ResetFactorySettings()
        {
            gameObject.SetActive(defaultSetActive);
            rigidbody.useGravity = defaultRigidbodyGravity;
            boxCollider.enabled = defaultEnabledCollider;
            boxCollider.isTrigger = defaultTriggerCollider;
            boxCollider.center = defaultColliderCenter;
            boxCollider.size = defaultColliderSize;
            trans.position = defaultPosition;
            trans.rotation = defaultRotation;
        }

        #endregion

    }
}
