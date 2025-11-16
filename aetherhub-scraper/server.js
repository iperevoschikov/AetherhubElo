import express, { json } from 'express';
import { chromium } from 'playwright';

const app = express();
const PORT = process.env.PORT || 8080;

app.use(json());

async function scrapeContent(url, timeout = 30000) {
    console.log(`Запуск скрапинга для URL: ${url}`);
    console.log(`Ожидание селектора: #Tourneys`);

    const browser = await chromium.launch({
        headless: true,
        args: ['--no-sandbox', '--disable-setuid-sandbox']
    });

    try {
        const context = await browser.newContext({
            userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
        });

        const page = await context.newPage();

        console.log('Переход на страницу...');
        await page.goto(url, { waitUntil: 'domcontentloaded', timeout });

        console.log('Ожидание селектора #Tourneys...');
        await page.waitForSelector('#Tourneys', { timeout });

        console.log('Ожидание загрузки списка турниров...');
        await page.locator('#Tourneys tr').nth(19).waitFor({ timeout });

        console.log('Селектор найден! Получение контента...');
        const content = await page.content();

        await context.close();
        return content;
    } catch (error) {
        console.error('Ошибка при скрапинге:', error.message);
        throw error;
    } finally {
        await browser.close();
    }
}

app.listen(PORT, '0.0.0.0', () => {
    console.log(`Playwright scraper сервер запущен на порту ${PORT}`);
    console.log(`Endpoints:`);
    console.log(`  GET  /?url=<URL>&timeout=<ms>`);
});
