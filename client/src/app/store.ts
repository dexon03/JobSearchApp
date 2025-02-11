import { AnyAction, combineReducers, configureStore } from '@reduxjs/toolkit'
import { vacancyApi } from './features/vacancy/vacancy.api'
import { setupListeners } from '@reduxjs/toolkit/dist/query/react'
import { profileApi } from './features/profile/profile.api'
import { companyApi } from './features/company/company.api'
import { candidateApi } from './features/candidate/candidate.api'
import profileReducer from './slices/profile.slice'
import storage from 'redux-persist/lib/storage'
import { persistReducer, persistStore } from 'redux-persist'
import { usersApi } from './features/users/usersApi'
import { candidateResumeApi } from './features/profile/candidateResume.api'
import { chatApi } from './features/chat/chat.api'



const persistConfig = {
  key: 'root',
  storage,
  blacklist: ['candidateResumeApi', 'chatApi']
};

const appReducer = combineReducers({
  profile: profileReducer,
  [chatApi.reducerPath]: chatApi.reducer,
  [candidateResumeApi.reducerPath]: candidateResumeApi.reducer,
  [usersApi.reducerPath]: usersApi.reducer,
  [vacancyApi.reducerPath]: vacancyApi.reducer,
  [profileApi.reducerPath]: profileApi.reducer,
  [companyApi.reducerPath]: companyApi.reducer,
  [candidateApi.reducerPath]: candidateApi.reducer,
});

// Define state type before using it
type StateType = ReturnType<typeof appReducer>;

const rootReducer = (state: StateType | undefined, action: AnyAction) => {
  if (action.type === 'SIGNOUT_REQUEST') {
    storage.removeItem('persist:root')
    return appReducer(undefined, action);
  }
  return appReducer(state, action);
}

const persistedReducer = persistReducer(persistConfig, rootReducer);


export const store = configureStore({
  reducer: persistedReducer,
  middleware(getDefaultMiddleware) {
    return getDefaultMiddleware().concat(
      chatApi.middleware,
      candidateResumeApi.middleware,
      usersApi.middleware,
      vacancyApi.middleware,
      profileApi.middleware,
      companyApi.middleware,
      candidateApi.middleware
    )
  },
})

setupListeners(store.dispatch)

export const persistor = persistStore(store);

// Infer the `RootState` and `AppDispatch` types from the store itself
export type RootState = ReturnType<typeof store.getState>
// Inferred type: {posts: PostsState, comments: CommentsState, users: UsersState}
export type AppDispatch = typeof store.dispatch