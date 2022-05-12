import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';

import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  baseUrl = environment.apiUrL;
  private currentUserSorce = new ReplaySubject<User>(1);
  currentUser$= this.currentUserSorce

  constructor(private http: HttpClient) { }

  register(model:any){
    return this.http.post(this.baseUrl +'account/register',model).pipe(
      map((user:User) =>{
        if (user){
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSorce.next(user);
        }
      })
    )
  }

  login(model: any){

    return this.http.post(this.baseUrl +'account/login', model).pipe(
      map((response:User) => {
        const user= response;
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSorce.next(user)
        }
      })
    )
  }

 setCurrentUser(user:User) {
   this.currentUserSorce.next(user);
 }

  logout(){
    localStorage.removeItem('user');
    this.currentUserSorce.next(null);
  }
}
