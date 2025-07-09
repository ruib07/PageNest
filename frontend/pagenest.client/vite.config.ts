import tailwindcss from "@tailwindcss/vite";
import plugin from '@vitejs/plugin-react';
import { env } from 'process';
import { defineConfig } from 'vite';
import svgr from "vite-plugin-svgr";

export default defineConfig({
    plugins: [
        plugin(),
        tailwindcss(),
        svgr({
            svgrOptions: {
                icon: true,
                exportType: "named",
                namedExport: "ReactComponent",
            },
        }),
    ],
    server: {
        port: parseInt(env.DEV_SERVER_PORT || '3000'),
    }
})
