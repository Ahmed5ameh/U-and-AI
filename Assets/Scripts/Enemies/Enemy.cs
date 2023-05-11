using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Enemies
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private GameObject player;
        [SerializeField] private LayerMask groundLayer, playerLayer;
        [SerializeField][Range(0, 100)] private int enemyMaxHealth;
        private NavMeshAgent agent;
        private Animator animator;
        private MeshCollider meshCollider;
        private Vector3 initialPosition;
    
        /* Patroling */
        [SerializeField] private Vector3 walkPoint;
        [SerializeField] private float walkPointRange;
        [SerializeField] float detectionAngel = 75f;
        private bool walkPointSet;
    
        /* Attacking */
        [SerializeField] private float timeBetweenAttacks;
        private bool alreadyAttacked;
    
        /* States */
        [SerializeField] private float sightRange, attackRange;
        private bool playerInSightRange, playerInAttackRange;
        private bool isChasing;
        private bool isDead;

        /* Animator */
        private static readonly int AAttacking = Animator.StringToHash("Attacking");
        private static readonly int AWalking = Animator.StringToHash("Walking");
        private static readonly int ARun = Animator.StringToHash("Run");
        private static readonly int ADie = Animator.StringToHash("Die");

        #region Unity Functions

            private void Awake()
            {
                agent = GetComponent<NavMeshAgent>();
                player = GameObject.FindGameObjectWithTag("Player");
                animator = GetComponent<Animator>();
                meshCollider = GetComponent<MeshCollider>();
                initialPosition = transform.position;
            }

            private void Update()
            {
                if (isDead) return;

                var position = transform.position;
                var toPlayer = player.transform.position - position;
                
                //Debug.Log("toPlayer is " + toPlayer);
                //Debug.Log("toPlayer.magnitude is " + toPlayer.magnitude);
                //Debug.Log("Angle is " + Vector3.Angle(toPlayer.normalized, transform.forward));
                //Debug.Log("Dot product is " + Vector3.Dot(toPlayer.normalized, transform.forward));
            
                // Calculate sight range and attack range
                playerInSightRange = Physics.CheckSphere(position, sightRange, playerLayer) &&
                                     Vector3.Dot(toPlayer.normalized, transform.forward) > Mathf.Cos(detectionAngel * 0.5f * Mathf.Deg2Rad);
                
                playerInAttackRange = Physics.CheckSphere(position, attackRange, playerLayer);
                
                // If player is not in sight range nor in attack range => Patrol
                if (!playerInSightRange && !playerInAttackRange) Patrol();
                
                // If player gets in sight range => Chase him
                if (playerInSightRange && !playerInAttackRange && !isChasing) ChasePlayer();
                
                // If player gets in attack range => Attack him
                if (playerInSightRange && playerInAttackRange) AttackPlayer();
                
                
                //Debug.Log("playerInSightRange = " + playerInSightRange);
                //Debug.Log("playerInAttackRange = " + playerInAttackRange);
                //Debug.Log("canChase = " + canChase);
            }
            
            // Trying to animate the mesh
            private void LateUpdate()
            {
                meshCollider.sharedMesh = animator.gameObject.GetComponent<MeshFilter>().sharedMesh;
                //Debug.Log("Mesh Updated");
            }

            private void OnCollisionEnter(Collision collision)
            {
                // Check for the Projectile Layer from the "P_LPSP_PROJ_Bullet_01" prefab (bullet)
                if (collision.gameObject.layer == 15)
                {
                    Die();
                }
            }

            /* Visualization */
            private void OnDrawGizmos()
            {
                var toPlayer = player.transform.position - transform.position;
                
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, attackRange);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, sightRange);
                
// So it doesnt cause a bug while building the game                
#if UNITY_EDITOR
                Handles.color = Color.red;
                Handles.DrawSolidArc(
                    transform.position,
                    Vector3.up,
                    Quaternion.Euler(0, -detectionAngel * 0.5f, 0) * transform.forward,
                    detectionAngel,
                    sightRange
                );
                
                // Forward
                Handles.color = Color.blue;
                Handles.DrawLine(transform.position, transform.forward + transform.position);
                
                // To player
                Handles.color = Color.green;
                Handles.DrawLine(transform.position, player.transform.position);
#endif
                
            }

        #endregion

        
        
        #region Userdefined Functions

            private void Patrol()
            {
                agent.speed = 1;
                agent.acceleration = 1;
                animator.SetBool(ARun, false);

                if (!walkPointSet) SearchForWalkPoint();
                if (walkPointSet)
                {
                    animator.SetBool(AWalking, true);
                    agent.SetDestination(walkPoint);
                    //Debug.Log("Patroling");
                }

                // Check if we reached the waypoint (Calculate the distance)
                var distanceToWalkPoint = transform.position - walkPoint;
                if (distanceToWalkPoint.magnitude < 1f)
                {
                    agent.SetDestination(transform.position);
                    walkPointSet = false;
                }
            }

            private void ChasePlayer()
            {
                isChasing = true;
                animator.SetBool(AAttacking, false);
                agent.speed = 2;
                agent.acceleration = 2;
                var playerPosition = player.transform.position;
                agent.SetDestination(playerPosition);
                transform.LookAt(playerPosition);
                animator.SetBool(ARun, true);
                isChasing = false;
                //Debug.Log("Chasing");
            }

            private void AttackPlayer()
            {
                isChasing = false;

                var savedPlayerPos = player.transform.position;
            
                // Stop enemy from moving
                agent.SetDestination(transform.position);
                transform.LookAt(savedPlayerPos);

                if (!alreadyAttacked)
                {   
                    ///////////////////////////////////////////////////
                    // Attack code here
                    ///////////////////////////////////////////////////
                    
                    animator.SetBool(AAttacking, true);
                    //Debug.Log("Attacking");
                
                    ///////////////////////////////////////////////////
                    // Attack code here
                    ///////////////////////////////////////////////////
                
                    alreadyAttacked = true;
                    Invoke(nameof(ResetAttack), timeBetweenAttacks);
                }
            }
            


            private void SearchForWalkPoint()
            {
                // Calculate a random point in range
                var randomZ = Random.Range(-walkPointRange, walkPointRange);
                var randomX = Random.Range(-walkPointRange, walkPointRange);
            
                // Set the new random point
                var position = transform.position;
                walkPoint = new Vector3(
                    position.x + randomX,
                    position.y,
                    position.z + randomZ
                );
            
                // Check if this point is on the ground and not outside of the map
                /* Why "Water" layer? Because there is a bug when I choose any user defined layers (Objects go invisible for an unknown reason) */
                if (Physics.Raycast(walkPoint, -transform.up, 2, groundLayer))      
                    walkPointSet = true;
            }

            private void ResetAttack()
            {
                alreadyAttacked = false;
            }

            public void TakeDamage(int dmg)
            {
                enemyMaxHealth -= dmg;
                if (enemyMaxHealth <= 0)
                {
                    Die();
                }
            }

            private void Die()
            {
                if (isDead) return;
                
                // Stop enemy from moving
                agent.SetDestination(transform.position);
                animator.SetTrigger(ADie);
                isDead = true;
                Invoke(nameof(DestroyEnemy), 5);
                Invoke(nameof(Revive), Random.Range(7, 10));
            }

            private void DestroyEnemy()
            {
                gameObject.SetActive(false);
                gameObject.GetComponent<MeshCollider>().enabled = false;
            }

            private void Revive()
            {
                gameObject.SetActive(true);
                gameObject.GetComponent<MeshCollider>().enabled = true;
                transform.position = initialPosition;
                isDead = false;
            }
        
        
            // IEnumerator Wait(int seconds)
            // {
            //     yield return new WaitForSeconds(seconds);
            // }
            //
            // IEnumerator WaitAndROAR()
            // {
            //     animator.SetTrigger("Roaring");
            //     yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            // }

        #endregion
    
    }
}
