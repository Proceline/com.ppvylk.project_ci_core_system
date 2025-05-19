using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Audio;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem
{
    public class AilmentHandler : Object
    {
        public static void HandleTurnStart(GameTeam InTeam)
        {
            List<GridUnit> GridUnits = TacticBattleManager.GetUnitsOnTeam(InTeam);
            foreach (GridUnit unit in GridUnits)
            {
                if(unit)
                {
                    List<Ailment> ailments = unit.GetAilmentContainer().GetAilments();
                    foreach (Ailment currAilment in ailments)
                    {
                        if(currAilment)
                        {
                            AilmentExecutionInfo StartExecution = currAilment.m_ExecuteOnStartOfTurn;
                            HandleAilmentExecution(unit, StartExecution);
                        }
                    }

                    unit.GetAilmentContainer().IncrementAllAilments();
                    unit.GetAilmentContainer().CheckAilments();
                }
            }

            List<LevelCellBase> levelCells = TacticBattleManager.GetGrid().GetAllCells();
            foreach (LevelCellBase currLevelCell in levelCells)
            {
                if (currLevelCell)
                {
                    GridUnit unit = currLevelCell.GetUnitOnCell();
                    
                    AilmentContainer ailmentContainer = currLevelCell.GetAilmentContainer();
                    if (ailmentContainer)
                    {
                        List<AilmentContainedData> ailments = ailmentContainer.GetAllAilmentContainerData();

                        for (int i = 0; i < ailments.Count; i++)
                        {
                            AilmentContainedData currAilment = ailments[i];
                            if (currAilment.m_ailment)
                            {
                                AilmentExecutionInfo StartExecution = currAilment.m_ailment.m_ExecuteOnStartOfTurn;
                                HandleCellAilmentExecution(currAilment.m_AssociatedCell, StartExecution);

                                GridUnit Caster = currAilment.m_CastedBy;
                                if (Caster)//If there is a caster, then check if it's their turn to increment the ailment
                                {
                                    bool bShouldIncrementAilment = InTeam == Caster.GetTeam();
                                    if (bShouldIncrementAilment)
                                    {
                                        ailmentContainer.IncrementAilment(currAilment);
                                    }
                                }
                                else//If there was no caster, check if it was friendly
                                {
                                    if (InTeam == GameTeam.Friendly)
                                    {
                                        ailmentContainer.IncrementAilment(currAilment);
                                    }
                                }
                            }
                        }
                        
                        ailmentContainer.CheckAilments();
                    }
                }
            }
        }

        public static void HandleTurnEnd(GameTeam InTeam)
        {
            List<GridUnit> GridUnits = TacticBattleManager.GetUnitsOnTeam(InTeam);
            foreach (GridUnit unit in GridUnits)
            {
                if (unit)
                {
                    List<Ailment> ailments = unit.GetAilmentContainer().GetAilments();
                    foreach (Ailment currAilment in ailments)
                    {
                        if (currAilment)
                        {
                            AilmentExecutionInfo EndExecution = currAilment.m_ExecuteOnEndOfTurn;
                            HandleAilmentExecution(unit, EndExecution);
                        }
                    }
                }
            }

            List<LevelCellBase> levelCells = TacticBattleManager.GetGrid().GetAllCells();
            foreach (LevelCellBase currLevelCell in levelCells)
            {
                if (currLevelCell)
                {
                    GridUnit unit = currLevelCell.GetUnitOnCell();
                    if(unit && unit.GetTeam() == InTeam)
                    {
                        AilmentContainer ailmentContainer = currLevelCell.GetAilmentContainer();
                        if (ailmentContainer)
                        {
                            List<AilmentContainedData> ailments = ailmentContainer.GetAllAilmentContainerData();
                            foreach (AilmentContainedData currAilment in ailments)
                            {
                                if (currAilment.m_ailment)
                                {
                                    AilmentExecutionInfo EndExecution = currAilment.m_ailment.m_ExecuteOnEndOfTurn;
                                    HandleCellAilmentExecution(currAilment.m_AssociatedCell, EndExecution);
                                }
                            }
                        }
                    }
                }
            }
        }


        public static void HandleUnitOnCell(GridUnit InUnit, LevelCellBase InCell)
        {
            if (InCell && InUnit)
            {
                AilmentContainer ailmentContainer = InCell.GetAilmentContainer();
                if (ailmentContainer)
                {
                    List<AilmentContainedData> ailments = ailmentContainer.GetAllAilmentContainerData();
                    foreach (AilmentContainedData currAilment in ailments)
                    {
                        if (currAilment.m_ailment)
                        {
                            CellAilment cellAilment = currAilment.m_ailment as CellAilment;
                            if(cellAilment)
                            {
                                HandleAilmentExecution(InUnit, cellAilment.m_ExecuteOnUnitOver);
                            }
                        }
                    }
                }
            }
        }

        static void HandleAilmentExecution(GridUnit InUnit, AilmentExecutionInfo InAilmentExecution)
        {
            foreach (AbilityParam abilityParam in InAilmentExecution.m_Params)
            {
                abilityParam.ApplyTo(null, InUnit);
            }

            foreach (AbilityParticle abilityParticle in InAilmentExecution.m_SpawnOnReciever)
            {
                Vector3 pos = InUnit.transform.position;
                Quaternion rot = abilityParticle.transform.rotation;
                AbilityParticle CreatedAbilityParticle = Instantiate(abilityParticle.gameObject, pos, rot).GetComponent<AbilityParticle>();
                CreatedAbilityParticle.Setup(null, null, InUnit.GetCell());
            }

            AudioClip audioClip = InAilmentExecution.m_AudioClip;
            if (audioClip)
            {
                AudioPlayData audioData = new AudioPlayData(audioClip);
                AudioHandler.PlayAudio(audioData, InUnit.gameObject.transform.position);
            }
        }

        static void HandleCellAilmentExecution(LevelCellBase InCell, AilmentExecutionInfo InAilmentExecution)
        {
            GridUnit unitOnCell = InCell.GetUnitOnCell();
            if(unitOnCell)
            {
                foreach (AbilityParam abilityParam in InAilmentExecution.m_Params)
                {
                    abilityParam.ApplyTo(null, unitOnCell);
                }

                foreach (AbilityParticle abilityParticle in InAilmentExecution.m_SpawnOnReciever)
                {
                    Vector3 pos = unitOnCell.transform.position;
                    Quaternion rot = abilityParticle.transform.rotation;
                    AbilityParticle CreatedAbilityParticle = Instantiate(abilityParticle.gameObject, pos, rot).GetComponent<AbilityParticle>();
                    CreatedAbilityParticle.Setup(null, null, unitOnCell.GetCell());
                }

                AudioClip audioClip = InAilmentExecution.m_AudioClip;
                if (audioClip)
                {
                    AudioPlayData audioData = new AudioPlayData(audioClip);
                    AudioHandler.PlayAudio(audioData, InCell.gameObject.transform.position);
                }
            }
        }
    }
}
