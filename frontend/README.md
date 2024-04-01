
## Developing

Create a `.env` file or use `.env.example`, change the parameters and rename it to `.env`. 
To run your **frontend** server, use `npm run dev` and the node server will start on the port you specified in the `.env` file

## Building

To create a production version of your app run `npm run build`

You can preview the production build with `npm run preview`

> To deploy your app, you may need to install an **[adapter](https://kit.svelte.dev/docs/adapters)** for your target environment (I left my nginx configuration as a example). If you want to start the server, run: 
```node -r dotenv/config build```