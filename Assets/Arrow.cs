using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 10f;
    public Transform tip;

    private Rigidbody _rigidBody;
    private bool _inAir = false;
    private Vector3 _lastPosition = Vector3.zero;

    private ParticleSystem _particleSystem;
    private TrailRenderer _trailRenderer;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        _trailRenderer = GetComponentInChildren<TrailRenderer>();

        pullInteraction.PullActionReleased += Release;
        Stop();
    }

    private void Destroy()
    {
        pullInteraction.PullActionReleased -= Release;
    }

    private void Release(float value)
    {
        pullInteraction.PullActionReleased -= Release;
        gameObject.transform.parent = null;
        _inAir = true;
        SetPhysics(true);

        Vector3 force = transform.forward * value * speed;
        _rigidBody.AddForce(force, ForceMode.Impulse);

        StartCoroutine(RotateWithVelocity());
        _lastPosition = tip.position;

        _particleSystem.Play();
        _trailRenderer.emitting = true;
    }

    private IEnumerator RotateWithVelocity()
    {
        yield return new WaitForFixedUpdate();
        while(_inAir)
        {
            Quaternion NewRotation = Quaternion.LookRotation(_rigidBody.velocity, transform.up);
            transform.rotation = NewRotation;
            yield return null;
        }
    }

    void FixedUpdate()
    {
        if(_inAir)
        {
            CheckCollision();
            _lastPosition = tip.position;
        }
    }

    private void CheckCollision()
    {
        if (Physics.Linecast(_lastPosition, tip.position, out RaycastHit hitInfo))
        {
            if (hitInfo.transform.gameObject.layer != 8)
            {
                if (hitInfo.transform.TryGetComponent(out Rigidbody body))
                {
                    _rigidBody.interpolation = RigidbodyInterpolation.None;
                    transform.parent = hitInfo.transform;
                    body.AddForce(_rigidBody.velocity, ForceMode.Impulse);
                }
                Stop();
            }
        }
    }

        private void Stop()
        {
            _inAir = false;
            SetPhysics(false);

        _particleSystem.Stop();
        _trailRenderer.emitting = false;
        }

        private void SetPhysics(bool usePhysics)
        {
            _rigidBody.useGravity = usePhysics;
            _rigidBody.isKinematic = !usePhysics;
        }
   
}
