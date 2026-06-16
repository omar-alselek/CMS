import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { Chart, registerables } from 'chart.js'; // ← أضف هذا السطر

Chart.register(...registerables); // ← أضف هذا السطر لتسجيل كل أنواع الشارتات

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
