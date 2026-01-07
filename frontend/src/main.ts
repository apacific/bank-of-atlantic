import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import "./style.css";

/**
 * Initialize and mount the Vue application.
 * Sets up the main App component with router integration.
 * Mounts to the #app element in the HTML document.
 */
createApp(App).use(router).mount("#app");
