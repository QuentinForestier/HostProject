using UnityEngine;

namespace Host
{
    public class SpinnerAnimation : MonoBehaviour
    {
        // Number of time to do one rotation
        public float Speed = 1f;

        private float _elapsedTime = 0f;

        // Number of segments in the spinner
        private int _segments = 8;

        private int _rotation = 0;

        void Update()
        {
            _elapsedTime += Time.deltaTime;

            float segmentSpeed = Speed / _segments;

            if (_elapsedTime > segmentSpeed)
            {
                _rotation += 1;
                _elapsedTime -= segmentSpeed;
                transform.Rotate(0f, 0f, 360f / _segments);
            }

            if(_rotation >= _segments)
            {
                _rotation -= _segments;
            }
        }
    }
}