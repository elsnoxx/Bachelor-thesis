Table User {
  id uuid [pk, note: 'Primární klíč – UUID']
  username varchar(100) [not null, unique]
  email varchar(255)
  password_hash varchar(255)
  created_at datetime [default: `CURRENT_TIMESTAMP`]
  last_login datetime
}

Table GameRoom {
  id uuid [pk, note: 'Primární klíč – UUID']
  name varchar(100) [not null]
  // např. "relax_game", "focus_game"
  game_type varchar(50) [not null] 
  max_players int [default: 2]
  // waiting, active, finished
  status varchar(20) [default: 'waiting'] 
  // pokud null, tak room je public
  password_hash varchar(255)
  created_at datetime [default: `CURRENT_TIMESTAMP`]
  // user_id zakladatele roomu
  created_by uuid [not null]
}

Table Session {
  id uuid [pk, note: 'Primární klíč – UUID']
  user_id uuid [not null]
  game_room_id uuid [not null]
  start_time datetime [default: `CURRENT_TIMESTAMP`]
  end_time datetime
  is_active boolean [default: true]
}

Table BioFeedback {
  id int [pk, increment, note: 'Int kvůli úspoře paměti – datový log']
  session_id uuid [not null]
  gsr_value float
  timestamp datetime [default: `CURRENT_TIMESTAMP`]
}

Table Statistic {
  id uuid [pk, note: 'Primární klíč – UUID']
  user_id uuid [not null]
  game_type varchar(50)
  average_gsr float
  best_score float
  total_sessions int [default: 0]
  last_played datetime
}

Table RefRefreshToken{
  id uuid [pk]
  user_id uuid [not null]
  token varchar(255) [not null]
  Created datetime [default: `CURRENT_TIMESTAMP`]
  expires datetime [not null]
  revoked datetime [null]
  revorkedByIp varchar(255) [null]
  ReplacedByToken varchar(255) [null]
}

// Vztahy
Ref: Session.user_id > User.id
Ref: Session.game_room_id > GameRoom.id
Ref: BioFeedback.session_id > Session.id
Ref: Statistic.user_id > User.id
Ref: GameRoom.created_by > User.id  
