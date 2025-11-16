FROM mcr.microsoft.com/playwright:v1.40.0-jammy

# Установка рабочей директории
WORKDIR /app

# Копирование package.json
COPY package.json ./

# Установка зависимостей
RUN npm install

# Копирование скрипта
COPY server.js ./

# Открытие порта (будет переопределено переменной окружения)
EXPOSE 3000

# Запуск сервера
CMD ["node", "server.js"]