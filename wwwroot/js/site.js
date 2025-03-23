document.addEventListener('DOMContentLoaded', function() {
    // Toggle sidebar
    const sidebarCollapse = document.getElementById('sidebarCollapse');
    const sidebar = document.getElementById('sidebar');
    const content = document.getElementById('content');

    sidebarCollapse.addEventListener('click', function() {
        sidebar.classList.toggle('active');
        content.classList.toggle('active');
    });

    // Active link handling
    const currentLocation = location.href;
    const menuItems = document.querySelectorAll('#sidebar a');
    menuItems.forEach(item => {
        if(item.href === currentLocation){
            item.classList.add('active');
            const parent = item.parentElement.parentElement;
            if(parent.classList.contains('collapse')){
                parent.classList.add('show');
            }
        }
    });

    // Initialize all dropdowns
    const dropdowns = document.querySelectorAll('.dropdown-toggle');
    dropdowns.forEach(dropdown => {
        new bootstrap.Dropdown(dropdown);
    });
});