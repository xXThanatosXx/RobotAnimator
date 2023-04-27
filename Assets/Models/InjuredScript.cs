using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class InjuredScript : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI healthText;

    [SerializeField]
    private float maximumInjuredLayerWeight;

    private float maximumHealth = 100;
    private float currentHealth;

    private Animator animator;
    private int injuredLayerIndex;
    private float layerWeightVelocity;



    //Move

    public float speed;
    public float rotationSpeed;
    public float jumpSpeed;
    public float jumpButtonGracePeriod;

    private CharacterController characterController;
    private float ySpeed;
    private float originalStepOffset;
    private float? lastGroundedTime;
    private float? jumpButtonPressedTime;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maximumHealth;
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        injuredLayerIndex = animator.GetLayerIndex("Injured");
        originalStepOffset = characterController.stepOffset;
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput);
        float magnitude = Mathf.Clamp01(movementDirection.magnitude) * speed;
        movementDirection.Normalize();

        ySpeed += Physics.gravity.y * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.E))
        {
            currentHealth -= maximumHealth / 10;

            if (currentHealth < 0)
            {
                currentHealth = maximumHealth;
            }
        }


        //Luego se verifica si el personaje está en contacto con el suelo o no, y se actualiza "lastGroundedTime" si lo está.
        if (characterController.isGrounded)
        {
            lastGroundedTime = Time.time;
        }
        //Si se presiona el botón de salto, se actualiza "jumpButtonPressedTime"
        if (Input.GetButtonDown("Jump"))
        {
            jumpButtonPressedTime = Time.time;

        }


        //Si ha pasado menos tiempo que el periodo de gracia desde el último contacto con el suelo y desde la última vez
        //que se presionó el botón de salto, se ajusta la velocidad en el eje Y del personaje para saltar
        if (Time.time - lastGroundedTime <= jumpButtonGracePeriod)
        {
            characterController.stepOffset = originalStepOffset;
            ySpeed = -0.5f;


            if (Time.time - jumpButtonPressedTime <= jumpButtonGracePeriod)
            {
                ySpeed = jumpSpeed;
                jumpButtonPressedTime = null;
                lastGroundedTime = null;

            }
        }
        else
        {
            characterController.stepOffset = 0;
        }

        Vector3 velocity = movementDirection * magnitude;
        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        if (movementDirection != Vector3.zero)
        {
            animator.SetBool("Walk_Anim", true);
            //Crea una rotación con las direcciones en X,Y,Z.
            // eje Z se alineará con forward, el eje X se alineará con el producto cruz entre forward y up, y el eje Y se alineará con el producto cruzado entre Z y X.
            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);

            //Gira la matriz de transformación un paso más cerca de la del objetivo
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            animator.SetBool("Walk_Anim", false);

        }

        //Health function

        float healthPercentage = currentHealth / maximumHealth;
        healthText.text = $"Health: {healthPercentage * 100}%";

        float currentInjuredLayerWeight = animator.GetLayerWeight(injuredLayerIndex);
        float targetInjuredLayerWeight = (1 - healthPercentage) * maximumInjuredLayerWeight;
        animator.SetLayerWeight(
            injuredLayerIndex,
            Mathf.SmoothDamp(
                currentInjuredLayerWeight,
                targetInjuredLayerWeight,
                ref layerWeightVelocity,
                0.2f)
            );
    }
}
