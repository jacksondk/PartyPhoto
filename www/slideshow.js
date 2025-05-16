// File picker and image preview functionality
document.addEventListener('DOMContentLoaded', () => {
    // Every 15 seconds, call getrandomimage and update image
    setInterval(() => {
        fetch('/getrandomimage')
            .then(response => response.json())
            .then(data => {
                const imageUrl = data.imageUrl;
                const imagePreview = document.getElementById('image-preview');
                imagePreview.src = imageUrl;
            })
            .catch(error => console.error('Error fetching random image:', error));
    }, 15000);
});